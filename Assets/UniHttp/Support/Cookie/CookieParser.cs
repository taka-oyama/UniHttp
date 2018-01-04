using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace UniHttp
{
	internal sealed class CookieParser
	{
		HttpResponse response;

		internal CookieParser(HttpResponse response)
		{
			this.response = response;
		}

		internal List<CookieData> Parse()
		{
			List<CookieData> setCookies = new List<CookieData>();

			if(response.Headers.Exist("set-cookie")) {
				List<string> setCookiesStr = response.Headers["set-cookie"];
				setCookies.AddRange(setCookiesStr.Select(s => ParseEach(s)));
			}
			return setCookies;
		}

		CookieData ParseEach(string attributeStr)
		{
			var cookie = new CookieData();
			var attributes = attributeStr.Split(new[]{ "; " }, StringSplitOptions.None);
			var kvPair = attributes[0].Split('=');

			cookie.name = kvPair[0];
			cookie.value = kvPair[1];
			cookie.CreatedAt = DateTime.Now;

			foreach(var attr in attributes.Skip(1)) {
				kvPair = attr.Split('=');
				switch(kvPair[0]) {
				case "Domain": cookie.domain = kvPair[1];break;
				case "Path": cookie.path = kvPair[1]; break;
				case "Expires": cookie.expires = DateTime.Parse(kvPair[1]); break;
				case "Max-Age": cookie.expires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(kvPair[1])); break;
				case "Secure": cookie.secure = true; break;
				case "HttpOnly": cookie.httpOnly = true; break;
				}
			}

			if(string.IsNullOrEmpty(cookie.domain)) {
				cookie.ExactMatchOnly = true;
				cookie.domain = response.Request.Uri.Host;
			}
			if(string.IsNullOrEmpty(cookie.path)) {
				cookie.path = response.Request.Uri.AbsolutePath;
			}

			return cookie;
		}
	}
}