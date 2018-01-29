using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	internal sealed class RequestHandler
	{
		readonly HttpSettings settings;
		readonly CookieJar cookieJar;
		readonly CacheHandler cacheHandler;

		internal RequestHandler(HttpSettings settings, CookieJar cookieJar, CacheHandler cacheHandler)
		{
			this.settings = settings;
			this.cookieJar = cookieJar;
			this.cacheHandler = cacheHandler;
		}

		internal void Prepare(HttpRequest request)
		{
			/**
			 * Request has to be locked here to consider a case where a single instance of HttpRequest
			 * was sent multiple times simultaneously.
			 * 
			 * Ex.
			 * HttpRequest request = new HttpRequest(...);
			 * httpManager.Send(request);
			 * httpManager.Send(request);
			 */
			lock(request) {
				request.useProxy = settings.proxy != null;

				if(request.Headers.NotContains(HeaderField.Host)) {
					request.Headers.Add(HeaderField.Host, GenerateHost(request.Uri));
				}
				if(settings.allowResponseCompression && request.Headers.NotContains(HeaderField.AcceptEncoding)) {
					request.Headers.Add(HeaderField.AcceptEncoding, "gzip");
				}
				if(settings.appendDefaultUserAgentToRequest && request.Headers.NotContains(HeaderField.UserAgent)) {
					request.Headers.Add(HeaderField.UserAgent, UserAgent.value);
				}
				if(settings.useCookies) {
					AddCookiesToRequest(request);
				}
				if(!settings.useCache) {
					request.Headers.AddOrReplace(HeaderField.CacheControl, "no-store");
				}
				if(cacheHandler.IsCachable(request)) {
					request.cache = cacheHandler.FindMetadata(request);
					AddCacheDirectiveToRequest(request);
				}
				if(request.Data != null) {
					if(request.Headers.NotContains(HeaderField.ContentType)) {
						request.Headers.Add(HeaderField.ContentType, request.Data.GetContentType());
					}
					if(request.Headers.NotContains(HeaderField.ContentLength)) {
						request.Headers.Add(HeaderField.ContentLength, request.Data.ToBytes().Length.ToString());
					}
				}
			}
		}

		internal void Send(HttpRequest request, HttpStream stream)
		{
			byte[] data;
			lock(request) {
				data = request.ToBytes();
			}
			stream.Write(data, 0, data.Length);
			stream.Flush();
		}

		string GenerateHost(Uri uri)
		{
			string host = uri.Host;
			if(uri.Scheme == Uri.UriSchemeHttp && uri.Port != 80 || uri.Scheme == Uri.UriSchemeHttps && uri.Port != 443) {
				host += ":" + uri.Port;
			}
			return host;
		}

		void AddCookiesToRequest(HttpRequest request)
		{
			Dictionary<string, string> cookies = new Dictionary<string, string>();

			if(request.Headers.Contains(HeaderField.Cookie)) {
				string cookieString = request.Headers[HeaderField.Cookie];
				string[] presets = cookieString.Split(new string[] { "=", "; " }, StringSplitOptions.None);
				for(int i = 0; presets.Length > i; i += 2) {
					cookies.Add(presets[i], presets[i + 1]);
				}
			}

			foreach(Cookie cookie in cookieJar.FindMatch(request.Uri)) {
				if(!cookies.ContainsKey(cookie.name)) {
					cookies.Add(cookie.name, cookie.value);
				}
			}

			if(cookies.Count > 0) {
				StringBuilder sb = new StringBuilder();
				foreach(KeyValuePair<string, string> kv in cookies) {
					sb.Append(kv.Key);
					sb.Append("=");
					sb.Append(kv.Value);
					sb.Append("; ");
				}
				sb.Remove(sb.Length - 2, 2);
				request.Headers.AddOrReplace(HeaderField.Cookie, sb.ToString());
			}
		}

		void AddCacheDirectiveToRequest(HttpRequest request)
		{
			CacheMetadata cache = cacheHandler.FindMetadata(request);

			request.Headers.Remove(HeaderField.IfNoneMatch);
			request.Headers.Remove(HeaderField.IfModifiedSince);

			if(cache == null) {
				return;
			}

			if(!string.IsNullOrEmpty(cache.eTag)) {
				request.Headers.AddOrReplace(HeaderField.IfNoneMatch, cache.eTag);
			}
			if(cache.lastModified.HasValue) {
				request.Headers.AddOrReplace(HeaderField.IfModifiedSince, cache.lastModified.Value.ToString("r"));
			}
		}
	}
}
