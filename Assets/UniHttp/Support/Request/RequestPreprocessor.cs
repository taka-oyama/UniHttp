using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	internal sealed class RequestPreprocessor
	{
		HttpSettings settings;
		CookieJar cookieJar;
		CacheHandler cacheHandler;

		internal RequestPreprocessor(HttpSettings settings, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.settings = settings;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal void Execute(HttpRequest request)
		{
			if(settings.useCookies) {
				AddCookiesToRequest(request);
			}
			if(!settings.useCache) {
				request.Headers.AddOrReplace("Cache-Control", "no-store");
			}
			if(cacheHandler.IsCachable(request)) {
				AddCacheDirectiveToRequest(request);
			}
			if(request.Data != null) {
				if(request.Headers.NotExist("Content-Type")) {
					request.Headers.Add("Content-Type", request.Data.GetContentType());
				}
				if(request.Headers.NotExist("Content-Length")) {
					request.Headers.Add("Content-Length", request.Data.ToBytes().Length.ToString());
				}
			}
		}

		void AddCookiesToRequest(HttpRequest request)
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
				if(request.Headers.NotExist("If-None-Match")) {
					request.Headers.Add("If-None-Match", cache.eTag);
				}
			}
			if(cache.lastModified.HasValue) {
				DateTimeOffset modifiedSince = cache.lastModified.Value + cache.lastModified.Value.Offset;
				if(request.Headers.NotExist("If-Modified-Since")) {
					request.Headers.Add("If-Modified-Since", modifiedSince.ToString("r"));
				}
			}
		}
	}
}
