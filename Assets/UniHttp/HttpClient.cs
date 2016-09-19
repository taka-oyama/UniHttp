using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace UniHttp
{
	public class HttpClient
	{
		CookieJar cookieJar;

		public bool UseCookies;

		public HttpClient()
		{
			this.cookieJar = HttpDispatcher.CookieJar;
			this.UseCookies = true;
		}

		public HttpResponse Get(Uri uri, RequestHeaders headers = null, object payload = null)
		{
			return Send(new HttpRequest(uri, HttpMethod.GET, headers, payload));
		}

		public HttpResponse Send(HttpRequest request)
		{
			if(UseCookies) {
				AppendCookies(request);
			}
			var response = new HttpConnection(request).Send();
			if(UseCookies) {
				cookieJar.AddRange(new CookieParser(response).Parse());
				cookieJar.SaveToFile();
			}
			return response;
		}

		void AppendCookies(HttpRequest request)
		{
			var cookies = new Dictionary<string, string>();
			cookieJar.FindMatch(request.Uri).ForEach(c => cookies.Add(c.Name, c.Value));

			if(request.Headers.Exist("Cookie")) {
				foreach(var str in request.Headers["Cookie"].Split(new string[]{"; "}, StringSplitOptions.RemoveEmptyEntries)) {
					string[] kv = str.Split('=');
					cookies.Add(kv[0].Trim(), kv[1].Trim());
				}
			}

			var targets = cookies.Select(kv => kv.Key + "=" + kv.Value).ToArray();
			request.Headers.AddOrReplace("Cookie", string.Join("", targets));
		}
	}
}
