using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;

namespace UniHttp
{
	internal sealed class CookieJar
	{
		readonly FileStore fileStore;
		readonly CookieParser parser;
		readonly Dictionary<string, CookieDomain> jar;

		internal CookieJar(IFileHandler fileHandler, string dataDirectory)
		{
			this.fileStore = new FileStore(fileHandler, dataDirectory + "/Cookie.bin");
			this.parser = new CookieParser();
			this.jar = ReadFromFile();
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
						jar.Add(cookie.domain, new CookieDomain(response.Request.Uri.Host));
					}
					jar[cookie.domain].AddOrReplace(cookie);
				}
			}
		}

		internal void Clear()
		{
			lock (jar) {
				jar.Clear();
			}
		}

		internal void RemoveExpiredCookies()
		{
			lock(jar) {
				foreach(string key in jar.Keys) {
					jar[key].RemoveExpiredCookies();
				}
			}
		}

		internal void SaveToFile()
		{
			lock(jar) {
				RemoveExpiredCookies();
				Dictionary<string, List<Cookie>> saveable = new Dictionary<string, List<Cookie>>();
				foreach(string key in jar.Keys) {
					saveable.Add(key, jar[key].FindPersistedCookies());
				}
				fileStore.Write(saveable);
			}
		}

		internal Dictionary<string, CookieDomain> ReadFromFile()
		{
			if(!fileStore.Exists) {
				return new Dictionary<string, CookieDomain>();
			}
			try {
				return fileStore.Read<Dictionary<string, CookieDomain>>();
			}
			catch(IOException) {
				return new Dictionary<string, CookieDomain>();
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
