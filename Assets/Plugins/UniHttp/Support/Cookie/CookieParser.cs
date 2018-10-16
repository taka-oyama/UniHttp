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

			if(response.Headers.Contains(HeaderField.SetCookie)) {
				foreach(string attributesAsString in response.Headers[HeaderField.SetCookie]) {
					Cookie cookie = ParseEach(attributesAsString);
					if(string.IsNullOrEmpty(cookie.Path)) {
						cookie.Path = response.Request.Uri.AbsolutePath;
					}
					setCookies.Add(cookie);
				}
			}

			return setCookies;
		}

		Cookie ParseEach(string attributesAsString)
		{
			Cookie cookie = new Cookie();
			string[] attributes = attributesAsString.Split(new[]{ "; " }, StringSplitOptions.None);
			string[] kvPair = attributes[0].Split('=');

			cookie.Name = kvPair[0];
			cookie.Value = kvPair[1];
			cookie.Size = Encoding.ASCII.GetByteCount(attributesAsString);

			foreach(string attr in attributes.Skip(1)) {
				kvPair = attr.Split('=');
				switch(kvPair[0].ToLower()) {
				case "domain": cookie.Domain = kvPair[1]; break;
				case "path": cookie.Path = kvPair[1]; break;
				case "expires": cookie.Expires = DateTime.Parse(kvPair[1]); break;
				case "max-age": cookie.Expires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(kvPair[1])); break;
				case "secure": cookie.Secure = true; break;
				case "httponly": cookie.HttpOnly = true; break;
				}
			}

			return cookie;
		}
	}
}