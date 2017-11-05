using UnityEngine;
using System.Net.Sockets;
using System;

namespace UniHttp
{
	internal class Messenger
	{
		HttpSettings settings;
		StreamPool streamPool;
		ResponseHandler responseHandler;
		RequestHandler requestHandler;

		internal Messenger(HttpSettings settings, StreamPool streamPool, CacheHandler cacheHandler, CookieJar cookieJar)
		{
			this.settings = settings;
			this.streamPool = streamPool;
			this.requestHandler = new RequestHandler(settings, cookieJar, cacheHandler);
			this.responseHandler = new ResponseHandler(settings, cookieJar, cacheHandler);
		}

		internal HttpResponse Send(HttpRequest request)
		{
			HttpResponse response = null;
			HttpStream stream = null;

			while(true) {
				requestHandler.Prepare(request);

				settings.logger.Log(string.Concat(request.Uri, Constant.CRLF, request));

				try {
					stream = streamPool.CheckOut(request);
					requestHandler.Send(request, stream);
					response = responseHandler.Process(request, stream);
				}
				catch(SocketException exception) {
					response = responseHandler.Process(request, exception);
				}
				finally {
					if(stream != null) {
						streamPool.CheckIn(response, stream);
					}
				}

				settings.logger.Log(string.Concat(response.Request.Uri, Constant.CRLF, response));

				if(IsRedirect(response)) {
					request = MakeRedirectRequest(response);
				} else {
					break;
				}
			}

			return response;
		}

		bool IsRedirect(HttpResponse response)
		{
			if(settings.followRedirects) {
				for(int i = 0; i < Constant.Redirects.Length; i++) {
					if(response.StatusCode == Constant.Redirects[i]) {
						return true;
					}
				}
			}
			return false;
		}

		HttpRequest MakeRedirectRequest(HttpResponse response)
		{
			Uri uri = new Uri(response.Headers["Location"][0]);
			HttpRequest request = response.Request;
			HttpMethod method = request.Method;
			if(response.StatusCode == StatusCode.SeeOther) {
				if(method == HttpMethod.POST || method == HttpMethod.PUT || method == HttpMethod.PATCH) {
					method = HttpMethod.GET;
				}
			}

			request.Headers.Remove("Host");

			return new HttpRequest(method, uri, request.Headers, request.Data);
		}

	}
}
