using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace UniHttp
{
	internal sealed class ResponseHandler
	{
		const char COLON = ':';
		const char SPACE = ' ';
		const char CR = '\r';
		const char LF = '\n';

		readonly HttpSettings settings;
		readonly CookieJar cookieJar;
		readonly CacheHandler cacheHandler;

		internal ResponseHandler(HttpSettings settings, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.settings = settings;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal HttpResponse Process(HttpRequest request, HttpStream source, Progress progress, CancellationToken cancellationToken)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = source.ReadTo(SPACE).TrimEnd(SPACE);
			response.StatusCode = int.Parse(source.ReadTo(SPACE).TrimEnd(SPACE));
			response.StatusPhrase = source.ReadTo(LF).TrimEnd();
			string name = source.ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			while(name != String.Empty) {
				string valuesStr = source.ReadTo(LF).TrimStart(SPACE).TrimEnd(CR, LF);
				response.Headers.Append(name, valuesStr);
				name = source.ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			}
			response.MessageBody = BuildMessageBody(response, source, progress, cancellationToken);
			ProcessCookie(response);
			ProcessCache(response);
			return response;
		}

		internal HttpResponse Process(HttpRequest request, CacheMetadata cache, Progress progress, CancellationToken cancellationToken)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "HTTP/1.1";
			response.StatusCode = 200;
			response.StatusPhrase = "OK (cache)";
			response.Headers.Append(HeaderField.ContentType, cache.contentType);
			response.MessageBody = BuildMessageBodyFromCache(response, progress, cancellationToken);
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

		byte[] BuildMessageBody(HttpResponse response, HttpStream source, Progress progress, CancellationToken cancellationToken)
		{
			if(response.StatusCode == StatusCode.NotModified) {
				return BuildMessageBodyFromCache(response, progress, cancellationToken);
			}
			if(response.Headers.Contains(HeaderField.TransferEncoding, "chunked")) {
				return BuildMessageBodyFromChunked(response, source, progress, cancellationToken);
			}
			if(response.Headers.Contains(HeaderField.ContentLength)) {
				return BuildMessageBodyFromContentLength(response, source, progress, cancellationToken);
			}
			throw new Exception("Could not determine how to read message body!");
		}

		byte[] BuildMessageBodyFromCache(HttpResponse response, Progress progress, CancellationToken cancellationToken)
		{
			using(CacheStream source = cacheHandler.GetMessageBodyStream(response.Request)) {
				MemoryStream destination = new MemoryStream();
				if(source.CanSeek) {
					progress.Start(source.BytesRemaining);
					source.CopyTo(destination, source.BytesRemaining, cancellationToken, progress);
				}
				else {
					progress.Start();
					source.CopyTo(destination, cancellationToken);
				}
				progress.Finialize();
				return destination.ToArray();
			}
		}

		byte[] BuildMessageBodyFromChunked(HttpResponse response, HttpStream source, Progress progress, CancellationToken cancellationToken)
		{
			MemoryStream destination = new MemoryStream();
			progress.Start();
			long chunkSize = ReadChunkSize(source);
			while(chunkSize > 0) {
				source.CopyTo(destination, chunkSize, cancellationToken, progress);
				source.SkipTo(LF);
				chunkSize = ReadChunkSize(source);
			}
			source.SkipTo(LF);
			progress.Finialize();
			return DecodeMessageBody(response, destination, cancellationToken);
		}

		byte[] BuildMessageBodyFromContentLength(HttpResponse response, HttpStream source, Progress progress, CancellationToken cancellationToken)
		{
			long contentLength = long.Parse(response.Headers[HeaderField.ContentLength][0]);
			MemoryStream destination = new MemoryStream();
			progress.Start(contentLength);
			source.CopyTo(destination, contentLength, cancellationToken, progress);
			progress.Finialize();
			return DecodeMessageBody(response, destination, cancellationToken);
		}

		byte[] DecodeMessageBody(HttpResponse response, MemoryStream messageStream, CancellationToken cancellationToken)
		{
			if(response.Headers.Contains(HeaderField.ContentEncoding, "gzip")) {
				messageStream.Seek(0, SeekOrigin.Begin);
				return DecodeMessageBodyAsGzip(messageStream, cancellationToken);
			}
			return messageStream.ToArray();
		}

		byte[] DecodeMessageBodyAsGzip(MemoryStream compressedStream, CancellationToken cancellationToken)
		{
			using(GzipDecompressStream gzipStream = new GzipDecompressStream(compressedStream)) {
				MemoryStream destination = new MemoryStream();
				gzipStream.CopyTo(destination, cancellationToken);
				return destination.ToArray();
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
				cookieJar.Update(response);
			}
		}

		void ProcessCache(HttpResponse response)
		{
			if(settings.useCache) {
				if(response.StatusCode == StatusCode.NotModified) {
					response.Headers.Append(HeaderField.ContentType, response.Request.cache.contentType);
				}
				if(cacheHandler.IsCachable(response)) {
					cacheHandler.CacheResponse(response);
				}
			}
		}
	}
}
