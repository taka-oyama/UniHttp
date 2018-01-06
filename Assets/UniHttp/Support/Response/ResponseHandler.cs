using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.IO.Compression;

namespace UniHttp
{
	internal sealed class ResponseHandler
	{
		const char COLON = ':';
		const char SPACE = ' ';
		const char CR = '\r';
		const char LF = '\n';

		HttpSettings settings;
		CookieJar cookieJar;
		CacheHandler cacheHandler;

		internal ResponseHandler(HttpSettings settings, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.settings = settings;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal HttpResponse Process(HttpRequest request, HttpStream source)
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

			// Post process for response
			ProcessCookie(response);
			ProcessCache(response);

			return response;
		}

		internal HttpResponse Process(HttpRequest request, Exception exception)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "HTTP/1.1";
			response.StatusCode = 0;
			response.StatusPhrase = exception.Message.Trim();
			response.Headers.Append(HeaderField.ContentType, "text/plain");
			response.MessageBody = Encoding.UTF8.GetBytes(string.Concat(exception.GetType(), CR, LF, exception.StackTrace));

			return response;
		}

		byte[] BuildMessageBody(HttpResponse response, HttpStream source)
		{
			if(response.StatusCode == StatusCode.NotModified) {
				return BuildMessageBodyFromCache(response);
			}

			if(response.Headers.Exist(HeaderField.TransferEncoding, "chunked")) {
				return BuildMessageBodyFromChunked(response, source);
			}

			if(response.Headers.Exist(HeaderField.ContentLength)) {
				return BuildMessageBodyFromContentLength(response, source);
			}

			throw new Exception("Could not determine how to read message body!");
		}

		byte[] BuildMessageBodyFromCache(HttpResponse response)
		{
			MemoryStream destination = new MemoryStream();
			using(CacheStream cacheStream = cacheHandler.GetReadStream(response.Request))
			{
				Progress progress = response.Request.DownloadProgress;
				progress.Start(cacheStream.Length);
				cacheStream.CopyTo(destination, cacheStream.Length, progress);
				progress.Finialize();
				return destination.ToArray();
			}
		}

		byte[] BuildMessageBodyFromChunked(HttpResponse response, HttpStream source)
		{
			MemoryStream destination = new MemoryStream();
			Progress progress = response.Request.DownloadProgress;
			progress.Start();
			long chunkSize = ReadChunkSize(source);
			while(chunkSize > 0) {
				source.CopyTo(destination, chunkSize, progress);
				source.SkipTo(LF);
				chunkSize = ReadChunkSize(source);
			}
			source.SkipTo(LF);
			progress.Finialize();
			return DecodeMessageBody(response, destination);
		}

		byte[] BuildMessageBodyFromContentLength(HttpResponse response, HttpStream source)
		{
			MemoryStream destination = new MemoryStream();
			Progress progress = response.Request.DownloadProgress;
			long contentLength = long.Parse(response.Headers[HeaderField.ContentLength][0]);
			progress.Start(contentLength);
			source.CopyTo(destination, contentLength, progress);
			progress.Finialize();
			return DecodeMessageBody(response, destination);
		}

		byte[] DecodeMessageBody(HttpResponse response, MemoryStream stream)
		{
			if(response.Headers.Exist(HeaderField.ContentEncoding, "gzip")) {
				return DecodeMessageBodyAsGzip(stream);
			} else {
				return stream.ToArray();
			}
		}

		byte[] DecodeMessageBodyAsGzip(MemoryStream stream)
		{
			byte[] buffer = new byte[Constant.CopyBufferSize];
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

		long ReadChunkSize(HttpStream source)
		{
			MemoryStream destination = new MemoryStream();
			source.ReadTo(destination, LF);
			string hexStr = Encoding.ASCII.GetString(destination.ToArray()).TrimEnd(CR, LF);
			return long.Parse(hexStr, NumberStyles.HexNumber);
		}

		void ProcessCookie(HttpResponse response)
		{
			if(settings.useCookies) {
				cookieJar.ParseAndUpdate(response);
			}
		}

		void ProcessCache(HttpResponse response)
		{
			if(settings.useCache) {
				if(response.StatusCode == StatusCode.NotModified) {
					response.CacheData = cacheHandler.Find(response.Request);
				}
				if(cacheHandler.IsCachable(response)) {
					response.CacheData = cacheHandler.CacheResponse(response);
				}
			}
		}
	}
}
