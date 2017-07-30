using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Globalization;
using System.IO.Compression;

namespace UniHttp
{
	internal sealed class ResponseBuilder
	{
		const char LF = '\n';
		const char CR = '\r';
		const char COLON = ':';

		HttpResponse response;
		Stream sourceStream;
		int bufferSize;
		MessageBodyDecoder bodyDecoder;

		internal ResponseBuilder(HttpRequest request, Stream sourceStream, int bufferSize = 1024)
		{
			this.response = new HttpResponse(request);
			this.sourceStream = sourceStream;
			this.bufferSize = bufferSize;
			this.bodyDecoder = new MessageBodyDecoder(response, bufferSize);
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
			response.HttpVersion = ReadTo(' ').TrimEnd(' ');
			response.StatusCode = int.Parse(ReadTo(' ').TrimEnd(' '));
			response.StatusPhrase = ReadTo(LF).TrimEnd();
		}

		void SetHeaders()
		{
			string name = ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			while(name != String.Empty) {
				string valuesStr = ReadTo(LF).TrimEnd(CR, LF);
				response.Headers.Append(name, valuesStr);
				name = ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			}
		}

		void SetMessageBody()
		{
			if(response.StatusCode == 304) {
				response.MessageBody = HttpManager.CacheStorage.Read(response.Request.Uri);
				return;
			}

			if(response.Headers.Exist("Transfer-Encoding", "chunked")) {
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
					CopyTo(destination, chunkSize);
					SkipTo(LF);
					chunkSize = ReadChunkSize();
				}
				return bodyDecoder.Decode(destination);
			}
		}

		byte[] ReadMessageBodyWithLength(int contentLength)
		{
			using(MemoryStream destination = new MemoryStream())
			{
				CopyTo(destination, contentLength);
				return bodyDecoder.Decode(destination);
			}
		}

		int ReadChunkSize()
		{
			using(MemoryStream destination = new MemoryStream(0)) {
				ReadTo(destination, LF);
				string hexStr = Encoding.ASCII.GetString(destination.ToArray()).TrimEnd(CR, LF);
				return int.Parse(hexStr, NumberStyles.HexNumber);
			}
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

		int SkipTo(params char[] stopChars)
		{
			int b;
			int count = 0;
			do {
				b = sourceStream.ReadByte();
				if(b == -1) break;
				count += 1;
			} while(stopChars.All(s => b != (int)s));
			return count;
		}

		void CopyTo(Stream destination, int count)
		{
			byte[] buffer = new byte[bufferSize];
			int remainingBytes = count;
			int readBytes = 0;
			while(remainingBytes > 0) {
				readBytes = sourceStream.Read(buffer, 0, Math.Min(buffer.Length, remainingBytes));
				destination.Write(buffer, 0, readBytes);
				remainingBytes -= readBytes;
			}
		}
	}
}
