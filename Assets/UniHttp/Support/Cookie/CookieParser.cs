using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace UniHttp
{
	internal class CookieParser
	{
		HttpResponse response;

		internal CookieParser(HttpResponse response)
		{
			this.response = response;
		}

		internal List<Cookie> Parse()
		{
			List<Cookie> setCookies = new List<Cookie>();

			if(response.Headers.Exist("set-cookie")) {
				List<string> setCookiesStr = response.Headers["set-cookie"];
				setCookies.AddRange(setCookiesStr.Select(s => ParseEach(s)));
			}
			return setCookies;
		}

		Cookie ParseEach(string attributeStr)
		{
			var cookie = new Cookie();
			var attributes = attributeStr.Split(new[]{ "; " }, StringSplitOptions.None);
			var kvPair = attributes[0].Split('=');

			cookie.Name = kvPair[0];
			cookie.Value = kvPair[1];
			cookie.CreatedAt = DateTime.Now;

			foreach(var attr in attributes.Skip(1)) {
				kvPair = attr.Split('=');
				switch(kvPair[0]) {
				case "Domain": cookie.Domain = kvPair[1];break;
				case "Path": cookie.Path = kvPair[1]; break;
				case "Expires": cookie.Expires = DateTime.Parse(kvPair[1]); break;
				case "Max-Age": cookie.Expires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(kvPair[1])); break;
				case "Secure": cookie.Secure = true; break;
				case "HttpOnly": cookie.HttpOnly = true; break;
				}
			}

			if(string.IsNullOrEmpty(cookie.Domain)) {
				cookie.ExactMatchOnly = true;
				cookie.Domain = response.Request.Uri.Host;
			}
			if(string.IsNullOrEmpty(cookie.Path)) {
				cookie.Path = response.Request.Uri.AbsolutePath;
			}

			return cookie;
		}
	}
}