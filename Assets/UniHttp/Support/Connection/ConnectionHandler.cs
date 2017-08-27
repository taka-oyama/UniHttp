using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

namespace UniHttp
{
	internal sealed class ConnectionHandler
	{
		static int[] REDIRECTS = new [] {301, 302, 303, 307, 308};

		HttpSetting setting;
		ILogger logger;
		HttpStreamPool streamPool;
		RequestPreprocessor requestProcessor;
		ResponsePostprocessor responseProcessor;

		internal ConnectionHandler(HttpSetting setting)
		{
			var cookieJar = HttpManager.CookieJar;
			var cacheHandler = HttpManager.CacheHandler;

			this.setting = setting;
			this.logger = HttpManager.Logger;
			this.streamPool = HttpManager.StreamPool;
			this.requestProcessor = new RequestPreprocessor(setting, cookieJar, cacheHandler);
			this.responseProcessor = new ResponsePostprocessor(setting, cookieJar, cacheHandler);
		}

		internal HttpResponse Send(HttpRequest request)
		{
			HttpResponse response;

			try {
				requestProcessor.Execute(request);

				while(true) {
					LogRequest(request);

					HttpStream stream = streamPool.CheckOut(request);
					byte[] data = request.ToBytes();
					stream.Write(data, 0, data.Length);
					stream.Flush();

					response = new ResponseBuilder(request, stream).Build();
					streamPool.CheckIn(response, stream);
					responseProcessor.Execute(response);
					LogResponse(response);

					if(setting.followRedirects && IsRedirect(response)) {
						request = MakeRedirectRequest(response);
					} else {
						break;
					}
				}
			}
			catch(SocketException exception) {
				response = BuildErrorResponse(request, exception);
			}

			return response;
		}

		HttpResponse BuildErrorResponse(HttpRequest request, Exception exception)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "Unknown";
			response.StatusCode = 0;
			response.StatusPhrase = exception.Message.Trim();
			response.MessageBody = Encoding.UTF8.GetBytes(exception.StackTrace);
			return response;
		}

		bool IsRedirect(HttpResponse response)
		{
			for(int i = 0; i < REDIRECTS.Length; i++) {
				if(response.StatusCode == REDIRECTS[i]) {
					return true;
				}
			}
			return false;
		}

		HttpRequest MakeRedirectRequest(HttpResponse response)
		{
			Uri uri = new Uri(response.Headers["Location"][0]);
			HttpRequest request = response.Request;
			HttpMethod method = request.Method;
			if(response.StatusCode == 303) {
				if(request.Method == HttpMethod.POST ||
				   request.Method == HttpMethod.PUT ||
				   request.Method == HttpMethod.PATCH) {
					method = HttpMethod.GET;
				}
			}
			return new HttpRequest(method, uri, request.Headers, request.Data);
		}

		void LogRequest(HttpRequest request)
		{
			logger.Log(string.Concat(request.Uri.ToString(), Constant.CRLF, request.ToString()));
		}

		void LogResponse(HttpResponse response)
		{
			logger.Log(string.Concat(response.Request.Uri.ToString(), Constant.CRLF, response.ToString()));
		}
	}
}
