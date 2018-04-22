using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UniHttp
{
	public static class Http
	{
		static readonly HttpManager defaultManager;

		static Http()
		{
			defaultManager = defaultManager ?? HttpManager.Initalize();
		}

		public static async Task<HttpResponse> DeleteAsync(Uri uri, IHttpData data = null)
		{
			return await defaultManager.SendAsync(new HttpRequest(HttpMethod.DELETE, uri, data));
		}

		public static async Task<HttpResponse> GetAsync(Uri uri, HttpQuery query = null)
		{
			return await defaultManager.SendAsync(new HttpRequest(HttpMethod.GET, uri, query));
		}

		public static async Task<HttpResponse> PatchAsync(Uri uri, IHttpData data = null)
		{
			return await defaultManager.SendAsync(new HttpRequest(HttpMethod.PATCH, uri, data));
		}

		public static async Task<HttpResponse> PostAsync(Uri uri, IHttpData data = null)
		{
			return await defaultManager.SendAsync(new HttpRequest(HttpMethod.POST, uri, data));
		}

		public static async Task<HttpResponse> PutAsync(Uri uri, IHttpData data = null)
		{
			return await defaultManager.SendAsync(new HttpRequest(HttpMethod.PUT, uri, data));
		}

		public static async Task<HttpResponse> SendAsync(HttpRequest request)
		{
			return await defaultManager.SendAsync(request);
		}
	}
}
