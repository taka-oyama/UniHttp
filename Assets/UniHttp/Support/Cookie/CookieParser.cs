using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace UniHttp
{
	internal sealed class CookieParser
	{
		internal List<Cookie> Parse(HttpResponse response)
		{
			List<Cookie> setCookies = new List<Cookie>();

			if(response.Headers.Exist(HeaderField.SetCookie)) {
				foreach(string attributesAsString in response.Headers[HeaderField.SetCookie]) {
					Cookie cookie = ParseEach(attributesAsString);
					if(string.IsNullOrEmpty(cookie.path)) {
						cookie.path = response.Request.Uri.AbsolutePath;
					}
					setCookies.Add(cookie);
				}
			}

			return setCookies;
		}

		internal List<Cookie> Parse(byte[] data)
		{
			List<Cookie> setCookies = new List<Cookie>();

			foreach(string attributesAsString in Encoding.ASCII.GetString(data).Split('\n')) {
				if(!string.IsNullOrEmpty(attributesAsString)) {
					setCookies.Add(ParseEach(attributesAsString));
				}
			}
			return setCookies;
		}

		Cookie ParseEach(string attributesAsString)
		{
			Cookie cookie = new Cookie();
			string[] attributes = attributesAsString.Split(new[]{ "; " }, StringSplitOptions.None);
			string[] kvPair = attributes[0].Split('=');

			cookie.name = kvPair[0];
			cookie.value = kvPair[1];
			cookie.original = attributesAsString;
			cookie.size = Encoding.ASCII.GetByteCount(attributesAsString);

			foreach(string attr in attributes.Skip(1)) {
				kvPair = attr.Split('=');
				switch(kvPair[0].ToLower()) {
				case "domain": cookie.domain = kvPair[1]; break;
				case "path": cookie.path = kvPair[1]; break;
				case "expires": cookie.expires = DateTime.Parse(kvPair[1]); break;
				case "max-age": cookie.expires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(kvPair[1])); break;
				case "secure": cookie.secure = true; break;
				case "httponly": cookie.httpOnly = true; break;
				}
			}

			return cookie;
		}
	}
}