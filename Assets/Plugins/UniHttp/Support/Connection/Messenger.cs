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
		readonly HttpContext context;
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

		internal Messenger(HttpContext context, StreamPool streamPool, CacheHandler cacheHandler, CookieJar cookieJar)
		{
			this.context = context;
			this.streamPool = streamPool;
			this.requestHandler = new RequestHandler(cookieJar, cacheHandler);
			this.responseHandler = new ResponseHandler(cookieJar, cacheHandler);
		}

		internal async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
		{
			HttpResponse response = null;
			DateTime then = DateTime.Now;

			request.Settings.FillWith(context);

			return await Task.Run(async () => {
				while(true) {
					requestHandler.Prepare(request);

					LogRequest(request);

					response = IsCacheAvailable(request)
						? await GetResponseFromCacheAsync(request, cancellationToken)
						: await GetResponseFromSocketAsync(request, cancellationToken);

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

		async Task<HttpResponse> GetResponseFromCacheAsync(HttpRequest request, CancellationToken cancellationToken)
		{
			try {
				return await responseHandler.ProcessAsync(request, request.Cache, cancellationToken);
			}
			catch(IOException exception) {
				return responseHandler.Process(request, exception);
			}
		}

		async Task<HttpResponse> GetResponseFromSocketAsync(HttpRequest request, CancellationToken cancellationToken)
		{
			HttpStream stream = null;
			HttpResponse response = null;

			try {
				stream = await streamPool.CheckOutAsync(request);
				await requestHandler.SendAsync(request, stream, cancellationToken);
				response = await responseHandler.ProcessAsync(request, stream, cancellationToken);
			}
			catch(SocketException exception) {
				response = responseHandler.Process(request, exception);
				stream?.Close();
			}
			catch(IOException exception) {
				response = responseHandler.Process(request, exception);
				stream?.Close();
			}
			finally {
				streamPool.CheckIn(response, stream);
			}

			return response;
		}

		bool IsCacheAvailable(HttpRequest request)
		{
			return request.Cache != null && request.Cache.IsFresh;
		}

		void LogRequest(HttpRequest request)
		{
			lock(request) {
				context.Logger.Log(request.Uri + Constant.CRLF + request);
			}
		}

		void LogResponse(HttpResponse response)
		{
			context.Logger.Log(response.Request.Uri + Constant.CRLF + response);
		}

		bool IsRedirect(HttpResponse response)
		{
			if(response.Request.Settings.FollowRedirects.Value) {
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
			return new HttpRequest(uri, request);
		}
	}
}
