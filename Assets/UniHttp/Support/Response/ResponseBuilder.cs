using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.IO.Compression;
using UnityEngine;

namespace UniHttp
{
	internal sealed class ResponseBuilder
	{
		const char COLON = ':';
		const char SPACE = ' ';
		const char CR = '\r';
		const char LF = '\n';

		CacheHandler cacheHandler;
		int bufferSize;

		internal ResponseBuilder(CacheHandler cacheHandler, int bufferSize = 1024)
		{
			this.cacheHandler = cacheHandler;
			this.bufferSize = bufferSize;
		}

		internal HttpResponse Build(HttpRequest request, HttpStream source)
		{
			DateTime then = DateTime.Now;

			HttpResponse response = new HttpResponse(request);

			// Status Line
			response.HttpVersion = source.ReadTo(SPACE).TrimEnd(SPACE);
			response.StatusCode = int.Parse(source.ReadTo(SPACE).TrimEnd(SPACE));
			response.StatusPhrase = source.ReadTo(LF).TrimEnd();

			// Headers
			string name = source.ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			while(name != String.Empty) {
				string valuesStr = source.ReadTo(LF).TrimEnd(CR, LF);
				response.Headers.Append(name, valuesStr);
				name = source.ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
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

		byte[] BuildMessageBody(HttpResponse response, HttpStream source)
		{
			if(response.StatusCode == 304) {
				return cacheHandler.RetrieveFromCache(response.Request);
			}

			using(MemoryStream destination = new MemoryStream()) {
				if(response.Headers.Exist("Transfer-Encoding", "chunked")) {
					int chunkSize = ReadChunkSize(source);
					while(chunkSize > 0) {
						source.CopyTo(destination, chunkSize);
						source.SkipTo(LF);
						chunkSize = ReadChunkSize(source);
					}
					return DecodeMessageBody(response, destination);
				}

				if(response.Headers.Exist("Content-Length")) {
					int contentLength = int.Parse(response.Headers["Content-Length"][0]);
					source.CopyTo(destination, contentLength);
					return DecodeMessageBody(response, destination);
				}

				throw new Exception("Could not determine how to read message body!");
			}
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
			using(MemoryStream destination = new MemoryStream()) {
				using(GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress)) {
					while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
						destination.Write(buffer, 0, readBytes);
					}
					return destination.ToArray();
				}
			}
		}

		int ReadChunkSize(HttpStream source)
		{
			using(MemoryStream destination = new MemoryStream()) {
				source.ReadTo(destination, LF);
				string hexStr = Encoding.ASCII.GetString(destination.ToArray()).TrimEnd(CR, LF);
				return int.Parse(hexStr, NumberStyles.HexNumber);
			}
		}
	}
}
