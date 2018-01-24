using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;

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

		internal HttpResponse Send(DispatchInfo info)
		{
			HttpRequest request = info.request;
			HttpStream stream = null;
			HttpResponse response = null;

			while(true) {
				requestHandler.Prepare(request);

				lock(request) {
					settings.logger.Log(request.Uri + Constant.CRLF + request);
				}

				try {
					stream = streamPool.CheckOut(request);
					requestHandler.Send(request, stream);
					response = responseHandler.Process(request, stream, info.DownloadProgress, info.cancellationToken);
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

				settings.logger.Log(response.Request.Uri + Constant.CRLF + response);

				if(IsRedirect(response)) {
					request = MakeRedirectRequest(response);
				} else {
					break;
				}
			}

			return response;
		}

		void CloseStreamIfExists(Stream stream)
		{
			if(stream != null) {
				stream.Close();
			}
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
