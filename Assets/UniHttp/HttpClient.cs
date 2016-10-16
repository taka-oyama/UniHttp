using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpClient
	{
		public HttpSetting setting;

		RequestPreprocessor requestProcessor;
		ResponsePostprocessor responseProcessor;

		public HttpClient(HttpSetting? setting = null)
		{
			this.setting = setting.HasValue ? setting.Value : HttpSetting.Default;
			var cookieJar = HttpDispatcher.CookieJar;
			var cacheHandler = HttpDispatcher.CacheHandler;

			this.requestProcessor = new RequestPreprocessor(this.setting, cookieJar, cacheHandler);
			this.responseProcessor = new ResponsePostprocessor(this.setting, cookieJar, cacheHandler);
		}

		public HttpResponse Get(Uri uri, RequestHeaders headers = null, object payload = null)
		{
			return Send(new HttpRequest(uri, HttpMethod.GET, headers, payload));
		}

		public HttpResponse Send(HttpRequest request)
		{
			requestProcessor.Execute(request);
			var response = new HttpConnection(request).Send();
			responseProcessor.Execute(response);
			return response;
		}
	}
}
