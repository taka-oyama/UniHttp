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
			if(setting.useCache && cacheHandler.IsCachable(response)) {
				cacheHandler.CacheResponse(response);
			}
		}
	}
}
