using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;

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
				if(!jar.ContainsKey(domain)) {
					jar.Add(domain, new CookieDomain(baseDirectory, domain, parser, fileHandler));
				}

				foreach(Cookie cookie in jar[domain]) {
					if(cookie.IsExpired) {
						continue;
					}
					if(cookie.secure && uri.Scheme != Uri.UriSchemeHttps) {
						continue;
					}
					// add to qualification only if the path matches
					if(uri.AbsolutePath.StartsWith(cookie.path)) {
						relevants.Add(cookie);
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
					string domain = response.Request.Uri.Host;
					if(!jar.ContainsKey(domain)) {
						jar.Add(domain, new CookieDomain(baseDirectory, domain, parser, fileHandler));
					}
					jar[domain].AddOrReplace(cookie);
				}
			}
		}

		internal void Clear()
		{
			lock(jar) {
				jar.Clear();
			}
		}

		internal void WriteToFile()
		{
			lock(jar) {
				foreach(string key in jar.Keys) {
					jar[key].WriteToFile();
				}
			}
		}
	}
}
