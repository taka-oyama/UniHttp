﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UniHttp
{
	internal sealed class CookieParser
	{
		internal List<Cookie> Parse(HttpResponse response)
		{
			List<Cookie> setCookies = new List<Cookie>();

			if(response.Headers.Exist(HeaderField.SetCookie)) {
				List<string> setCookiesStr = response.Headers[HeaderField.SetCookie];
				setCookies.AddRange(setCookiesStr.Select(attributesAsString => ParseEach(response, attributesAsString)));
			}
			return setCookies;
		}

		Cookie ParseEach(HttpResponse response, string attributesAsString)
		{
			Cookie cookie = new Cookie();
			string[] attributes = attributesAsString.Split(new[]{ "; " }, StringSplitOptions.None);
			string[] kvPair = attributes[0].Split('=');

			cookie.name = kvPair[0];
			cookie.value = kvPair[1];

			foreach(string attr in attributes.Skip(1)) {
				kvPair = attr.Split('=');
				switch(kvPair[0]) {
				case "Domain": cookie.domain = kvPair[1]; break;
				case "Path": cookie.path = kvPair[1]; break;
				case "Expires": cookie.expires = DateTime.Parse(kvPair[1]); break;
				case "Max-Age": cookie.expires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(kvPair[1])); break;
				case "Secure": cookie.secure = true; break;
				case "HttpOnly": cookie.httpOnly = true; break;
				}
			}

			if(string.IsNullOrEmpty(cookie.domain)) {
				cookie.exactMatchOnly = true;
				cookie.domain = response.Request.Uri.Host;
			}
			if(string.IsNullOrEmpty(cookie.path)) {
				cookie.path = response.Request.Uri.AbsolutePath;
			}

			return cookie;
		}
	}
}