using UnityEngine;

namespace UniHttp
{
	internal sealed class ResponsePostprocessor
	{
		HttpSettings settings;
		CookieJar cookieJar;
		CacheHandler cacheHandler;

		internal ResponsePostprocessor(HttpSettings settings, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.settings = settings;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal void Execute(HttpResponse response)
		{
			if(settings.useCookies) {
				cookieJar.AddOrReplaceRange(new CookieParser(response).Parse());
			}
			if(settings.useCache) {
				if(response.StatusCode == StatusCode.NotModified) {
					response.CacheInfo = cacheHandler.Find(response.Request);
				}
				if(cacheHandler.IsCachable(response)) {
					response.CacheInfo = cacheHandler.CacheResponse(response);
				}
			}
		}
	}
}
