using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Globalization;

namespace UniHttp
{
	internal class ResponseBuilder
	{
		const char LF = '\n';

		HttpResponse response;
		Stream sourceStream;
		int bufferSize;

		internal ResponseBuilder(HttpRequest request, Stream sourceStream, int bufferSize = 1024)
		{
			this.response = new HttpResponse(request);
			this.sourceStream = sourceStream;
			this.bufferSize = bufferSize;
		}

		internal HttpResponse Build()
		{
			DateTime then = DateTime.Now;

			SetStatusLine();
			SetHeaders();
			SetMessageBody();

			response.RoundTripTime = DateTime.Now - then;

			return response;
		}

		void SetStatusLine()
		{
			string line = ReadTo(LF).Trim();
			string[] sliced = line.Split(' ');
			response.HttpVersion = sliced[0];
			response.StatusCode = int.Parse(sliced[1]);
			response.StatusPhrase = string.Join(" ", sliced.Skip(2).ToArray());
		}

		void SetHeaders()
		{
			string name = ReadTo(':', LF);
			while(name != String.Empty) {
				string valuesStr = ReadTo(LF);
				string[] values = valuesStr.Split(';');
				foreach(string value in values) {
					response.Headers.Append(name.TrimEnd(':'), value.Trim());
				}
				name = ReadTo(':', LF).Trim();
			}
		}

		void SetMessageBody()
		{
			if(response.Headers.Exist("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")) {
				response.MessageBody = ReadMessageBodyChunked();
				return;
			}

			if(response.Headers.Exist("Content-Length")) {
				int length = int.Parse(response.Headers["Content-Length"][0]);
				response.MessageBody = ReadMessageBodyWithLength(length);
				return;
			}

			throw new Exception("Could not determine how to read message body!");
		}

		byte[] ReadMessageBodyChunked()
		{
			using(MemoryStream destination = new MemoryStream())
			{
				int chunkSize = ReadChunkSize();
				while(chunkSize > 0) {
					StreamHelper.CopyTo(sourceStream, destination, chunkSize, bufferSize, IsGzipped());
					StreamHelper.SkipTo(sourceStream, LF);
					chunkSize = ReadChunkSize();
				}
				return destination.ToArray();
			}
		}

		byte[] ReadMessageBodyWithLength(int contentLength)
		{
			using(MemoryStream destination = new MemoryStream())
			{
				StreamHelper.CopyTo(sourceStream, destination, contentLength, bufferSize, IsGzipped());
				return destination.ToArray();
			}
		}

		int ReadChunkSize()
		{
			using(MemoryStream destination = new MemoryStream(0)) {
				StreamHelper.ReadTo(sourceStream, destination, LF);
				string hexStr = Encoding.ASCII.GetString(destination.ToArray()).Trim();
				return int.Parse(hexStr, NumberStyles.HexNumber);
			}
		}

		string ReadTo(params char[] stoppers)
		{
			using(MemoryStream destination = new MemoryStream()) {
				StreamHelper.ReadTo(sourceStream, destination, stoppers);
				return Encoding.UTF8.GetString(destination.ToArray());
			}
		}

		bool IsGzipped()
		{
			return response.Headers.Exist("Content-Encoding") && response.Headers["Content-Encoding"].Contains("gzip");
		}
	}
}
