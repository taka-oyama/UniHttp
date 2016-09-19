using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Globalization;
using Unity.IO.Compression;

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
				response.Headers.Append(name.TrimEnd(':'), valuesStr.Trim());
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
					CopyTo(destination, chunkSize, bufferSize, IsGzipped());
					SkipTo(LF);
					chunkSize = ReadChunkSize();
				}
				return destination.ToArray();
			}
		}

		byte[] ReadMessageBodyWithLength(int contentLength)
		{
			using(MemoryStream destination = new MemoryStream())
			{
				CopyTo(destination, contentLength, bufferSize, IsGzipped());
				return destination.ToArray();
			}
		}

		int ReadChunkSize()
		{
			using(MemoryStream destination = new MemoryStream(0)) {
				ReadTo(destination, LF);
				string hexStr = Encoding.ASCII.GetString(destination.ToArray()).Trim();
				return int.Parse(hexStr, NumberStyles.HexNumber);
			}
		}

		bool IsGzipped()
		{
			return response.Headers.Exist("Content-Encoding") && response.Headers["Content-Encoding"].Contains("gzip");
		}

		string ReadTo(params char[] stoppers)
		{
			using(MemoryStream destination = new MemoryStream()) {
				return ReadTo(destination, stoppers);
			}
		}

		string ReadTo(MemoryStream destination, params char[] stoppers)
		{
			int b;
			int count = 0;
			do {
				b = sourceStream.ReadByte();
				destination.WriteByte((byte)b);
				if(b == -1) break;
				count += 1;
			} while(stoppers.All(s => b != (int)s));
			return Encoding.UTF8.GetString(destination.ToArray());
		}

		int SkipTo(params char[] stoppers)
		{
			int b;
			int count = 0;
			do {
				b = sourceStream.ReadByte();
				if(b == -1) break;
				count += 1;
			} while(stoppers.All(s => b != (int)s));
			return count;
		}

		void CopyTo(Stream destination, int count, int bufferSize = 1024, bool decompress = false)
		{
			byte[] buffer = new byte[bufferSize];
			int remainingBytes = count;
			int readBytes = 0;

			if(decompress) {
				using(GZipStream gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress)) {
					while(remainingBytes > 0) {
						while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
							destination.Write(buffer, 0, readBytes);
							remainingBytes -= readBytes;
						}
					}
				}
			} else {
				while(remainingBytes > 0) {
					readBytes = sourceStream.Read(buffer, 0, Math.Min(buffer.Length, remainingBytes));
					destination.Write(buffer, 0, readBytes);
					remainingBytes -= readBytes;
				}
			}
		}
	}
}
