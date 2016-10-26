﻿using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

namespace UniHttp
{
	internal class HttpMessanger
	{
		static int[] REDIRECTS = new [] {301, 302, 303, 307, 308};

		HttpSetting setting;
		HttpStreamPool connectionPool;
		RequestPreprocessor requestProcessor;
		ResponsePostprocessor responseProcessor;

		internal HttpMessanger(HttpSetting setting)
		{
			var cookieJar = HttpManager.CookieJar;
			var cacheHandler = HttpManager.CacheHandler;

			this.setting = setting;
			this.connectionPool = HttpManager.TcpConnectionPool;
			this.requestProcessor = new RequestPreprocessor(setting, cookieJar, cacheHandler);
			this.responseProcessor = new ResponsePostprocessor(setting, cookieJar, cacheHandler);

		}

		internal HttpResponse Send(HttpRequest request)
		{
			try {
				requestProcessor.Execute(request);
				var response = Transmit(request);
				responseProcessor.Execute(response);
				return response;
			}
			catch(SocketException exception) {
				return BuildErrorResponse(request, exception);
			}
		}

		HttpResponse Transmit(HttpRequest request)
		{
			HttpResponse response;
			HttpStream stream = connectionPool.CheckOut(request);
			while(true) {
				byte[] data = new RequestDataBuilder(request).Build();
				stream.Write(data, 0, data.Length);
				stream.Flush();

				response = new ResponseBuilder(request, stream).Build();
				if(setting.followRedirects && IsRedirect(response)) {
					request = ConstructRequest(response);
				} else {
					break;
				}
			}
			connectionPool.CheckIn(response, stream);
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

		HttpRequest ConstructRequest(HttpResponse response)
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
			return new HttpRequest(uri, method, request.Headers, request.Payload);
		}
	}
}
