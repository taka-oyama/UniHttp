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
		const char COLON = ':';
		const char SPACE = ' ';
		const char CR = '\r';
		const char LF = '\n';

		int bufferSize;

		internal ResponseBuilder(int bufferSize = 1024)
		{
			this.bufferSize = bufferSize;
		}

		internal HttpResponse Build(HttpRequest request, Stream source)
		{
			DateTime then = DateTime.Now;

			HttpResponse response = new HttpResponse(request);

			// Status Line
			response.HttpVersion = ReadTo(source, SPACE).TrimEnd(SPACE);
			response.StatusCode = int.Parse(ReadTo(source, SPACE).TrimEnd(SPACE));
			response.StatusPhrase = ReadTo(source, LF).TrimEnd();

			// Headers
			string name = ReadTo(source, COLON, LF).TrimEnd(COLON, CR, LF);
			while(name != String.Empty) {
				string valuesStr = ReadTo(source, LF).TrimEnd(CR, LF);
				response.Headers.Append(name, valuesStr);
				name = ReadTo(source, COLON, LF).TrimEnd(COLON, CR, LF);
			}

			// Message Body
			response.MessageBody = BuildMessageBody(response, source);

			// Roundtrip Time
			response.RoundTripTime = DateTime.Now - then;

			return response;
		}

		internal HttpResponse Build(HttpRequest request, Exception exception)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "HTTP/1.1";
			response.StatusCode = 0;
			response.StatusPhrase = exception.Message.Trim();
			response.Headers.Append("Content-Type", "text/plain");
			response.MessageBody = Encoding.UTF8.GetBytes(string.Concat(exception.GetType(), CR, LF, exception.StackTrace));

			return response;
		}

		byte[] BuildMessageBody(HttpResponse response, Stream source)
		{
			if(response.StatusCode == 304) {
				return HttpManager.CacheStorage.Read(response.Request.Uri);
			}

			if(response.Headers.Exist("Transfer-Encoding", "chunked")) {
				using(MemoryStream destination = new MemoryStream())
				{
					int chunkSize = ReadChunkSize(source);
					while(chunkSize > 0) {
						CopyTo(source, destination, chunkSize);
						SkipTo(source, LF);
						chunkSize = ReadChunkSize(source);
					}
					return DecodeMessageBody(response, destination);
				}
			}

			if(response.Headers.Exist("Content-Length")) {
				int contentLength = int.Parse(response.Headers["Content-Length"][0]);
				using(MemoryStream destination = new MemoryStream())
				{
					CopyTo(source, destination, contentLength);
					return DecodeMessageBody(response, destination);
				}
			}

			throw new Exception("Could not determine how to read message body!");
		}

		byte[] DecodeMessageBody(HttpResponse response, MemoryStream stream)
		{
			if(response.Headers.Exist("Content-Encoding", "gzip")) {
				return DecodeMessageBodyAsGzip(stream);
			} else {
				return stream.ToArray();
			}
		}

		byte[] DecodeMessageBodyAsGzip(MemoryStream stream)
		{
			byte[] buffer = new byte[bufferSize];
			int readBytes = 0;
			stream.Seek(0, SeekOrigin.Begin);
			using(var destination = new MemoryStream()) {
				using(var gzipStream = new GZipStream(stream, CompressionMode.Decompress)) {
					while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
						destination.Write(buffer, 0, readBytes);
					}
					return destination.ToArray();
				}
			}
		}

		int ReadChunkSize(Stream source)
		{
			using(MemoryStream destination = new MemoryStream(0)) {
				ReadTo(source, destination, LF);
				string hexStr = Encoding.ASCII.GetString(destination.ToArray()).TrimEnd(CR, LF);
				return int.Parse(hexStr, NumberStyles.HexNumber);
			}
		}

		string ReadTo(Stream source, params char[] stoppers)
		{
			using(MemoryStream destination = new MemoryStream()) {
				return ReadTo(source, destination, stoppers);
			}
		}

		string ReadTo(Stream source, MemoryStream destination, params char[] stoppers)
		{
			int b;
			int count = 0;
			do {
				b = source.ReadByte();
				destination.WriteByte((byte)b);
				if(b == -1) break;
				count += 1;
			} while(stoppers.All(s => b != (int)s));
			return Encoding.UTF8.GetString(destination.ToArray());
		}

		int SkipTo(Stream source, params char[] stopChars)
		{
			int b;
			int count = 0;
			do {
				b = source.ReadByte();
				if(b == -1) break;
				count += 1;
			} while(stopChars.All(s => b != (int)s));
			return count;
		}

		void CopyTo(Stream source, Stream destination, int count)
		{
			byte[] buffer = new byte[bufferSize];
			int remainingBytes = count;
			int readBytes = 0;
			while(remainingBytes > 0) {
				readBytes = source.Read(buffer, 0, Math.Min(buffer.Length, remainingBytes));
				destination.Write(buffer, 0, readBytes);
				remainingBytes -= readBytes;
			}
		}
	}
}
