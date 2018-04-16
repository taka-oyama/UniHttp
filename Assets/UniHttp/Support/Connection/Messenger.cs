using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace UniHttp
{
	internal class Messenger
	{
		readonly HttpSettings settings;
		readonly StreamPool streamPool;
		readonly ResponseHandler responseHandler;
		readonly RequestHandler requestHandler;

		static readonly int[] statusCodesForRedirect = new[] {
			StatusCode.MovedPermanently,
			StatusCode.Found,
			StatusCode.SeeOther,
			StatusCode.TemporaryRedirected,
			StatusCode.PermanentRedirect,
		};

		internal Messenger(HttpSettings settings, StreamPool streamPool, CacheHandler cacheHandler, CookieJar cookieJar)
		{
			this.settings = settings;
			this.streamPool = streamPool;
			this.requestHandler = new RequestHandler(settings, cookieJar, cacheHandler);
			this.responseHandler = new ResponseHandler(settings, cookieJar, cacheHandler);
		}

		internal async Task<HttpResponse> SendAsync(HttpRequest request, Progress progress, CancellationToken cancellationToken)
		{
			HttpResponse response = null;
			DateTime then = DateTime.Now;

			return await Task.Run(async () => {
				Debug.Log(Thread.CurrentThread.ManagedThreadId);
				while(true) {
					requestHandler.Prepare(request);

					LogRequest(request);

					response = IsCacheAvailable(request)
						? await GetResponseFromCacheAsync(request, progress, cancellationToken)
						: await GetResponseFromSocketAsync(request, progress, cancellationToken);

					response.Duration = DateTime.Now - then;

					LogResponse(response);

					if(IsRedirect(response)) {
						request = MakeRedirectRequest(response);
					}
					else {
						break;
					}
				}
				return response;
			});
		}

		async Task<HttpResponse> GetResponseFromCacheAsync(HttpRequest request, Progress progress, CancellationToken cancellationToken)
		{
			try {
				return await responseHandler.ProcessAsync(request, request.cache, progress, cancellationToken);
			}
			catch(IOException exception) {
				return responseHandler.Process(request, exception);
			}
		}

		async Task<HttpResponse> GetResponseFromSocketAsync(HttpRequest request, Progress progress, CancellationToken cancellationToken)
		{
			HttpStream stream = null;
			HttpResponse response = null;

			try {
				stream = await streamPool.CheckOutAsync(request);
				await requestHandler.SendAsync(request, stream, cancellationToken);
				response = await responseHandler.ProcessAsync(request, stream, progress, cancellationToken);
			}
			catch(SocketException exception) {
				response = responseHandler.Process(request, exception);
				CloseStreamIfExists(stream);
			}
			catch(IOException exception) {
				response = responseHandler.Process(request, exception);
				CloseStreamIfExists(stream);
			}
			finally {
				streamPool.CheckIn(response, stream);
			}

			return response;
		}

		bool IsCacheAvailable(HttpRequest request)
		{
			return request.cache != null && request.cache.IsFresh;
		}

		void CloseStreamIfExists(Stream stream)
		{
			if(stream != null) {
				stream.Close();
			}
		}

		void LogRequest(HttpRequest request)
		{
			lock(request) {
				settings.logger.Log(request.Uri + Constant.CRLF + request);
			}
		}

		void LogResponse(HttpResponse response)
		{
			settings.logger.Log(response.Request.Uri + Constant.CRLF + response);
		}

		bool IsRedirect(HttpResponse response)
		{
			if(settings.followRedirects) {
				for(int i = 0; i < statusCodesForRedirect.Length; i++) {
					if(response.StatusCode == statusCodesForRedirect[i]) {
						return true;
					}
				}
			}
			return false;
		}

		HttpRequest MakeRedirectRequest(HttpResponse response)
		{
			Uri uri = new Uri(response.Headers[HeaderField.Location][0]);
			HttpRequest request = response.Request;
			HttpMethod method = request.Method;
			if(response.StatusCode == StatusCode.SeeOther) {
				if(method == HttpMethod.POST || method == HttpMethod.PUT || method == HttpMethod.PATCH) {
					method = HttpMethod.GET;
				}
			}
			request.Headers.Remove(HeaderField.Host);
			return new HttpRequest(method, uri, null, request.Headers, request.Data);
		}
	}
}
