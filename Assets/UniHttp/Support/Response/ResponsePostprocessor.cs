using UnityEngine;

namespace UniHttp
{
	internal sealed class ResponsePostprocessor
	{
		HttpOptions option;
		CookieJar cookieJar;
		CacheHandler cacheHandler;

		internal ResponsePostprocessor(HttpOptions option, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.option = option;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal void Execute(HttpResponse response)
		{
			if(option.useCookies) {
				cookieJar.AddOrReplaceRange(new CookieParser(response).Parse());
			}
			if(option.useCache) {
				if(response.StatusCode == 304) {
					response.CacheInfo = cacheHandler.Find(response.Request);
				}
				if(cacheHandler.IsCachable(response)) {
					response.CacheInfo = cacheHandler.CacheResponse(response);
				}
			}
		}
	}
}
