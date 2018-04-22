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

		public static Task<HttpResponse> DeleteAsync(Uri uri, IHttpData data = null, Progress progress = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.DELETE, uri, data), progress);
		}

		public static Task<HttpResponse> GetAsync(Uri uri, HttpQuery query = null, Progress progress = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.GET, uri, query), progress);
		}

		public static Task<HttpResponse> PatchAsync(Uri uri, IHttpData data = null, Progress progress = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.PATCH, uri, data), progress);
		}

		public static Task<HttpResponse> PostAsync(Uri uri, IHttpData data = null, Progress progress = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.POST, uri, data), progress);
		}

		public static Task<HttpResponse> PutAsync(Uri uri, IHttpData data = null, Progress progress = null)
		{
			return DefaultManager.SendAsync(new HttpRequest(HttpMethod.PUT, uri, data), progress);
		}

		public static Task<HttpResponse> SendAsync(HttpRequest request, Progress progress = null)
		{
			return DefaultManager.SendAsync(request, progress);
		}
	}
}
