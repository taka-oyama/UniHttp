using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace UniHttp
{
	internal sealed class ResponseHandler
	{
		const char COLON = ':';
		const char SPACE = ' ';
		const char CR = '\r';
		const char LF = '\n';

		readonly CookieJar cookieJar;
		readonly CacheHandler cacheHandler;

		internal ResponseHandler(CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal async Task<HttpResponse> ProcessAsync(HttpRequest request, HttpStream source, CancellationToken cancellationToken)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = (await source.ReadToAsync(cancellationToken, SPACE)).TrimEnd(SPACE);
			response.StatusCode = int.Parse(source.ReadTo(SPACE).TrimEnd(SPACE));
			response.StatusPhrase = source.ReadTo(LF).TrimEnd();
			string name = source.ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			while(name != String.Empty) {
				string valuesStr = source.ReadTo(LF).TrimStart(SPACE).TrimEnd(CR, LF);
				response.Headers.Append(name, valuesStr);
				name = source.ReadTo(COLON, LF).TrimEnd(COLON, CR, LF);
			}
			response.MessageBody = await BuildMessageBody(response, source, cancellationToken);
			ProcessCookie(response);
			ProcessCache(response);
			return response;
		}

		internal async Task<HttpResponse> ProcessAsync(HttpRequest request, CacheMetadata cache, CancellationToken cancellationToken)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "HTTP/1.1";
			response.StatusCode = StatusCode.OK;
			response.StatusPhrase = "OK (cache)";
			response.Headers.Append(HeaderField.ContentType, cache.contentType);
			response.MessageBody = await BuildMessageBodyFromCacheAsync(response, cancellationToken);
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

		Task<byte[]> BuildMessageBody(HttpResponse response, HttpStream source, CancellationToken cancellationToken)
		{
			if(response.StatusCode == StatusCode.NotModified) {
				return BuildMessageBodyFromCacheAsync(response, cancellationToken);
			}
			if(response.Headers.Contains(HeaderField.TransferEncoding, "chunked")) {
				return BuildMessageBodyFromChunkedAsync(response, source, cancellationToken);
			}
			if(response.Headers.Contains(HeaderField.ContentLength)) {
				return BuildMessageBodyFromContentLengthAsync(response, source, cancellationToken);
			}
			throw new Exception("Could not determine how to read message body!");
		}

		async Task<byte[]> BuildMessageBodyFromCacheAsync(HttpResponse response, CancellationToken cancellationToken)
		{
			using(CacheStream source = cacheHandler.GetMessageBodyStream(response.Request)) {
				Progress progress = response.Request.Progress;
				MemoryStream destination = new MemoryStream();
				if(source.CanSeek) {
					progress?.Start(source.BytesRemaining);
					await source.CopyToAsync(destination, source.BytesRemaining, cancellationToken, progress);
				}
				else {
					progress?.Start();
					await source.CopyToAsync(destination, cancellationToken);
				}
				progress?.Finialize();
				return destination.ToArray();
			}
		}

		async Task<byte[]> BuildMessageBodyFromChunkedAsync(HttpResponse response, HttpStream source, CancellationToken cancellationToken)
		{
			Progress progress = response.Request.Progress;
			MemoryStream destination = new MemoryStream();
			progress?.Start();
			long chunkSize = await ReadChunkSizeAsync(source, cancellationToken);
			while(chunkSize > 0) {
				await source.CopyToAsync(destination, chunkSize, cancellationToken, progress);
				source.SkipTo(LF);
				chunkSize = await ReadChunkSizeAsync(source, cancellationToken);
			}
			source.SkipTo(LF);
			progress?.Finialize();
			return await DecodeMessageBodyAsync(response, destination, cancellationToken);
		}

		async Task<byte[]> BuildMessageBodyFromContentLengthAsync(HttpResponse response, HttpStream source, CancellationToken cancellationToken)
		{
			long contentLength = long.Parse(response.Headers[HeaderField.ContentLength][0]);
			Progress progress = response.Request.Progress;
			MemoryStream destination = new MemoryStream();
			progress?.Start(contentLength);
			await source.CopyToAsync(destination, contentLength, cancellationToken, progress);
			progress?.Finialize();
			return await DecodeMessageBodyAsync(response, destination, cancellationToken);
		}

		async Task<byte[]> DecodeMessageBodyAsync(HttpResponse response, MemoryStream messageStream, CancellationToken cancellationToken)
		{
			if(response.Headers.Contains(HeaderField.ContentEncoding, "gzip")) {
				messageStream.Seek(0, SeekOrigin.Begin);
				return await DecodeMessageBodyAsGzipAsync(messageStream, cancellationToken);
			}
			return messageStream.ToArray();
		}

		async Task<byte[]> DecodeMessageBodyAsGzipAsync(MemoryStream compressedStream, CancellationToken cancellationToken)
		{
			using(GzipDecompressStream gzipStream = new GzipDecompressStream(compressedStream)) {
				MemoryStream destination = new MemoryStream();
				await gzipStream.CopyToAsync(destination, cancellationToken);
				return destination.ToArray();
			}
		}

		async Task<long> ReadChunkSizeAsync(HttpStream source, CancellationToken cancellationToken)
		{
			MemoryStream destination = new MemoryStream();
			await source.ReadToAsync(destination, cancellationToken, LF);
			string hexStr = Encoding.ASCII.GetString(destination.ToArray()).TrimEnd(CR, LF);
			return long.Parse(hexStr, NumberStyles.HexNumber);
		}

		void ProcessCookie(HttpResponse response)
		{
			if(response.Request.Settings.UseCookies.Value) {
				cookieJar.Update(response);
			}
		}

		void ProcessCache(HttpResponse response)
		{
			if(response.Request.Settings.UseCache.Value) {
				if(response.StatusCode == StatusCode.NotModified) {
					response.Headers.Append(HeaderField.ContentType, response.Request.Cache.contentType);
				}
				if(cacheHandler.IsCachable(response)) {
					cacheHandler.CacheResponse(response);
				}
			}
		}
	}
}
