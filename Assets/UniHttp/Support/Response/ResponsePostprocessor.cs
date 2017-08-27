using UnityEngine;

namespace UniHttp
{
	internal sealed class ResponsePostprocessor
	{
		HttpSetting setting;
		CookieJar cookieJar;
		CacheHandler cacheHandler;

		internal ResponsePostprocessor(HttpSetting setting, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.setting = setting;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal void Execute(HttpResponse response)
		{
			if(setting.useCookies) {
				cookieJar.AddOrReplaceRange(new CookieParser(response).Parse());
			}
			if(setting.useCache) {
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
