using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace UniHttp
{
	internal sealed class RequestHandler
	{
		readonly CookieJar cookieJar;
		readonly CacheHandler cacheHandler;

		internal RequestHandler(CookieJar cookieJar, CacheHandler cacheHandler)
		{
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
				if(request.Headers.NotContains(HeaderField.Host)) {
					request.Headers.Add(HeaderField.Host, GenerateHost(request.Uri));
				}
				if(request.Settings.AllowCompressedResponse.Value && request.Headers.NotContains(HeaderField.AcceptEncoding)) {
					request.Headers.Add(HeaderField.AcceptEncoding, HeaderValue.Gzip);
				}
				if(request.Settings.AppendUserAgentToRequest.Value && request.Headers.NotContains(HeaderField.UserAgent)) {
					request.Headers.Add(HeaderField.UserAgent, UserAgent.value);
				}
				if(request.Settings.UseCookies.Value) {
					AddCookiesToRequest(request);
				}
				if(!request.Settings.UseCache.Value) {
					request.Headers.AddOrReplace(HeaderField.CacheControl, HeaderValue.NoStore);
				}
				if(cacheHandler.IsCachable(request)) {
					request.Cache = cacheHandler.FindMetadata(request);
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

        internal async Task SendAsync(HttpRequest request, HttpStream stream, CancellationToken cancellationToken)
		{
			MemoryStream source;
			lock(request) {
				source = new MemoryStream(request.ToBytes());
			}
			await stream.SendToAsync(source, cancellationToken);
			await stream.FlushAsync(cancellationToken);
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
				string[] presets = cookieString.Split(new string[] { Constant.Equal, "; " }, StringSplitOptions.None);
				for(int i = 0; presets.Length > i; i += 2) {
					cookies.Add(presets[i], presets[i + 1]);
				}
			}

			foreach(Cookie cookie in cookieJar.FindMatch(request.Uri)) {
				if(!cookies.ContainsKey(cookie.Name)) {
					cookies.Add(cookie.Name, cookie.Value);
				}
			}

			if(cookies.Count > 0) {
				StringBuilder sb = new StringBuilder();
				foreach(KeyValuePair<string, string> kv in cookies) {
					sb.Append(kv.Key);
					sb.Append(Constant.Equal);
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
