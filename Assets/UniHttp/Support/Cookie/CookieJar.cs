﻿using UnityEngine;
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
		readonly Dictionary<string, List<Cookie>> jar;

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

		internal void ParseAndUpdate(HttpResponse response)
		{
			List<Cookie> setCookies = parser.Parse(response);

			lock(jar) {
				foreach(Cookie setCookie in setCookies) {
					if(!jar.ContainsKey(setCookie.domain)) {
						jar.Add(setCookie.domain, new List<Cookie>());
					}
					Cookie target = jar[setCookie.domain].Find(c => c.name == setCookie.name);
					if(target != null) {
						jar[setCookie.domain].Remove(target);
					}
					if(!setCookie.IsExpired) {
						jar[setCookie.domain].Add(setCookie);
					}
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
					jar[key].RemoveAll(c => c.IsExpired);
				}
			}
		}

		List<Cookie> FindForDomain(Uri uri, string domain)
		{
			List<Cookie> relevants = new List<Cookie>();

			lock(jar) {
				if(jar.ContainsKey(domain)) {
					foreach(Cookie data in jar[domain]) {
						if(data.IsExpired) {
							continue;
						}
						if(data.secure && uri.Scheme != Uri.UriSchemeHttps) {
							continue;
						}
						if(data.exactMatchOnly && uri.Host != domain) {
							continue;
						}
						// add to qualification only if the path matches
						if(uri.AbsolutePath.IndexOf(data.path) == 0) {
							relevants.Add(data);
						}
					}
				}
			}

			return relevants;
		}

		internal void SaveToFile()
		{
			lock(jar) {
				RemoveExpiredCookies();
				Dictionary<string, List<Cookie>> saveable = new Dictionary<string, List<Cookie>>();
				foreach(string key in jar.Keys) {
					saveable.Add(key, jar[key].FindAll(c => !c.IsSession));
				}
				fileStore.Write(saveable);
			}
		}

		internal Dictionary<string, List<Cookie>> ReadFromFile()
		{
			if(!fileStore.Exists) {
				return new Dictionary<string, List<Cookie>>();
			}
			try {
				return fileStore.Read<Dictionary<string, List<Cookie>>>();
			}
			catch(IOException) {
				return new Dictionary<string, List<Cookie>>();
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
