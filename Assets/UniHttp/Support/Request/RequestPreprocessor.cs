using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	internal sealed class RequestPreprocessor
	{
		HttpSetting setting;
		CookieJar cookieJar;
		CacheHandler cacheHandler;

		internal RequestPreprocessor(HttpSetting setting, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.setting = setting;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal void Execute(HttpRequest request)
		{
			if(setting.useCookies) {
				AppendCookiesToRequest(request);
			}
			if(!setting.useCache) {
				request.Headers.AddOrReplace("Cache-Control", "no-store");
			}
			if(cacheHandler.IsCachable(request)) {
				AddCacheDirectiveToRequest(request);
			}
			if(request.Data != null) {
				request.Headers.AddOrReplace("Content-Type", request.Data.GetContentType());
				request.Headers.AddOrReplace("Content-Length", request.Data.ToBytes().Length.ToString());
			}
		}

		void AppendCookiesToRequest(HttpRequest request)
		{
			var cookies = new Dictionary<string, string>();
			cookieJar.FindMatch(request.Uri).ForEach(c => cookies.Add(c.name, c.value));

			if(request.Headers.Exist("Cookie")) {
				var presets = request.Headers["Cookie"].Split(new string[]{"=", "; "}, StringSplitOptions.None);
				for(int i = 0; presets.Length > 0; i += 2) {
					cookies.Add(presets[i], presets[i + 1]);
				}
			}

			if(cookies.Count > 0) {
				StringBuilder sb = new StringBuilder();
				foreach(var kv in cookies) {
					sb.Append(kv.Key);
					sb.Append("=");
					sb.Append(kv.Value);
					sb.Append(";");
				}
				sb.Remove(sb.Length - 1, 1);
				request.Headers.AddOrReplace("Cookie", sb.ToString());
			}
		}

		void AddCacheDirectiveToRequest(HttpRequest request)
		{
			CacheInfo cache = cacheHandler.Find(request);
			if(cache == null) {
				return;
			}
			if(!string.IsNullOrEmpty(cache.eTag)) {
				request.Headers.Add("If-None-Match", cache.eTag);
			}
			if(cache.lastModified.HasValue) {
				request.Headers.Add("If-Modified-Since", cache.lastModified.Value.ToString("r"));
			}
		}
	}
}
