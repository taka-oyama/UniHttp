using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UniHttp
{
	internal sealed class CookieJar
	{
		readonly IFileHandler fileHandler;
		readonly string baseDirectory;
		readonly CookieParser parser;
		readonly Dictionary<string, CookieDomain> jar;

		internal CookieJar(IFileHandler fileHandler, string dataDirectory)
		{
			this.fileHandler = fileHandler;
			this.baseDirectory = dataDirectory + "/Cookies";
			this.parser = new CookieParser();
			this.jar = new Dictionary<string, CookieDomain>();
		}

		internal List<Cookie> FindMatch(Uri uri)
		{
			List<Cookie> relevants = new List<Cookie>();

			IPAddress address;
			if(IPAddress.TryParse(uri.Host, out address)) {
				AddRelevantsForDomain(relevants, uri, uri.Host);
			} else {
				string domain = uri.Host;
				int index = 0;
				do {
					AddRelevantsForDomain(relevants, uri, domain);
					index = domain.IndexOf('.');
					domain = domain.Substring(index + 1);
				} while(index >= 0);
			}

			return relevants;
		}
		
		void AddRelevantsForDomain(List<Cookie> relevants, Uri uri, string domain)
		{
			lock(jar) {
				if(jar.ContainsKey(domain)) {
					foreach(Cookie cookie in jar[domain]) {
						if(cookie.IsExpired) {
							continue;
						}
						if(cookie.secure && uri.Scheme != Uri.UriSchemeHttps) {
							continue;
						}
						if(cookie.exactMatchOnly && uri.Host != domain) {
							continue;
						}
						// add to qualification only if the path matches
						if(uri.AbsolutePath.StartsWith(cookie.path)) {
							relevants.Add(cookie);
						}
					}
				}
			}
		}

		internal void Update(HttpResponse response)
		{
			if(response.Headers.NotExist(HeaderField.SetCookie)) {
				return;
			}

			List<Cookie> setCookies = parser.Parse(response);

			lock(jar) {
				foreach(Cookie cookie in setCookies) {
					if(!jar.ContainsKey(cookie.domain)) {
						jar.Add(cookie.domain, new CookieDomain(response, fileHandler, baseDirectory));
					}
					jar[cookie.domain].AddOrReplace(cookie);
				}
			}
		}

		internal void Clear()
		{
			lock(jar) {
				jar.Clear();
			}
		}

		internal void SaveToFile()
		{
			lock(jar) {
				foreach(string key in jar.Keys) {
					jar[key].SaveToFile();
				}
			}
		}

		public override string ToString()
		{
			lock(jar) {
				StringBuilder sb = new StringBuilder();
				foreach(string key in jar.Keys) {
					sb.Append(key + ":\n");
					foreach(Cookie cookie in jar[key]) {
						sb.Append("   " + cookie + "\n");
					}
				}
				return sb.ToString();
			}
		}
	}
}
