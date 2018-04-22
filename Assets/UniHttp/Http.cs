using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UniHttp
{
	public static class Http
	{
		static HttpManager defaultManager;

		static HttpManager DefaultManager
		{
			get { return defaultManager = defaultManager ?? HttpManager.Initalize(); }
		}

		public static Task<HttpResponse> DeleteAsync(Uri uri, IHttpData data = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.DELETE, uri, data));
		}

		public static Task<HttpResponse> GetAsync(Uri uri, HttpQuery query = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.GET, uri, query));
		}

		public static Task<HttpResponse> PatchAsync(Uri uri, IHttpData data = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.PATCH, uri, data));
		}

		public static Task<HttpResponse> PostAsync(Uri uri, IHttpData data = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.POST, uri, data));
		}

		public static Task<HttpResponse> PutAsync(Uri uri, IHttpData data = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.PUT, uri, data));
		}

		public static Task<HttpResponse> SendAsync(HttpRequest request)
		{
			return DefaultManager.SendAsync(request);
		}
	}
}
