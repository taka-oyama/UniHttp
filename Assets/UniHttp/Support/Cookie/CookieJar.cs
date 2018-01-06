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
		object locker;
		ObjectStorage io;
		CookieParser parser;
		Dictionary<string, List<Cookie>> cookies;

		internal CookieJar(IFileHandler fileHandler, string dataDirectory)
		{
			this.locker = new object();
			this.io = new ObjectStorage(fileHandler, dataDirectory + "/Cookie.bin");
			this.parser = new CookieParser();
			this.cookies = ReadFromFile();
		}

		internal List<Cookie> FindMatch(Uri uri)
		{
			List<Cookie> relevants = new List<Cookie>();
			IPAddress address;

			lock(locker) {
				if(IPAddress.TryParse(uri.Host, out address)) {
					relevants.AddRange(FindForDomain(uri, uri.Host));
				} else {
					string domain = uri.Host;
					int index = 0;
					do {
						relevants.AddRange(FindForDomain(uri, domain));
						index = domain.IndexOf('.');
						domain = domain.Substring(index + 1);
					} while(index >= 0);
				}
				return relevants;
			}
		}

		internal void ParseAndUpdate(HttpResponse response)
		{
			List<Cookie> newCookies = parser.Parse(response);

			lock(locker) {
				foreach(Cookie newCookie in newCookies) {
					if(!cookies.ContainsKey(newCookie.domain)) {
						cookies.Add(newCookie.domain, new List<Cookie>());
					}
					Cookie target = cookies[newCookie.domain].Find(c => c.name == newCookie.name);
					if(target != null) {
						cookies[newCookie.domain].Remove(target);
					}
					if(newCookie.expires == null || newCookie.expires >= DateTime.Now) {
						cookies[newCookie.domain].Add(newCookie);
					}
				}
			}
		}

		internal void CleanUp()
		{
			lock(locker) {
				foreach(string key in cookies.Keys) {
					cookies[key].RemoveAll(c => c.IsExpired);
				}
			}
		}

		internal void Clear()
		{
			lock(locker) {
				cookies.Clear();
			}
		}

		List<Cookie> FindForDomain(Uri uri, string domain)
		{
			List<Cookie> relevants = new List<Cookie>();

			if(cookies.ContainsKey(domain)) {
				foreach(Cookie data in cookies[domain]) {
					if(data.IsExpired) {
						continue;
					}
					if(data.secure && uri.Scheme != Uri.UriSchemeHttps) {
						continue;
					}
					if(data.ExactMatchOnly && uri.Host != domain) {
						continue;
					}
					// add to qualification only if the path matches
					if(uri.AbsolutePath.IndexOf(data.path) == 0) {
						relevants.Add(data);
					}
				}
			}

			return relevants;
		}

		internal void SaveToFile()
		{
			lock(locker) {
				CleanUp();
				Dictionary<string, List<Cookie>> saveable = new Dictionary<string, List<Cookie>>();
				foreach(string key in cookies.Keys) {
					saveable.Add(key, cookies[key].FindAll(c => !c.IsSession));
				}
				io.Write(saveable);
			}
		}

		internal Dictionary<string, List<Cookie>> ReadFromFile()
		{
			if(!io.Exists) {
				return new Dictionary<string, List<Cookie>>();
			}
			try {
				return io.Read<Dictionary<string, List<Cookie>>>();
			}
			catch(IOException) {
				return new Dictionary<string, List<Cookie>>();
			}
		}

		public override string ToString()
		{
			lock(locker) {
				StringBuilder sb = new StringBuilder();
				foreach(string key in cookies.Keys) {
					sb.Append(key + ":\n");
					foreach(Cookie cookie in cookies[key]) {
						sb.Append("   " + cookie.ToString() + "\n");
					}
				}
				return sb.ToString();
			}
		}
	}
}
