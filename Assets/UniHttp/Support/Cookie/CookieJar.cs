using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniHttp
{
	internal sealed class CookieJar
	{
		object locker;
		ObjectStorage io;
		Dictionary<string, List<CookieData>> data;

		internal CookieJar(IFileHandler fileHandler, string dataDirectory)
		{
			this.locker = new object();
			this.io = new ObjectStorage(fileHandler, dataDirectory + "/Cookie.bin");
			this.data = ReadFromFile();
		}

		internal List<CookieData> FindMatch(Uri uri)
		{
			List<CookieData> relevants = new List<CookieData>();
			IPAddress address;

			lock(locker) {
				if(IPAddress.TryParse(uri.Host, out address)) {
					relevants.AddRange(Fetch(uri, uri.Host));
				} else {
					string domain = uri.Host;
					int index = 0;
					do {
						relevants.AddRange(Fetch(uri, domain));
						index = domain.IndexOf('.');
						domain = domain.Substring(index + 1);
					} while(index >= 0);
				}
				return relevants;
			}
		}

		internal void AddOrReplaceRange(List<CookieData> cookies)
		{
			lock(locker) {
				cookies.ForEach(Add);
			}
		}

		internal void CleanUp()
		{
			lock(locker) {
				foreach(string key in data.Keys) {
					data[key].RemoveAll(c => c.IsExpired);
				}
			}
		}

		internal void Clear()
		{
			lock(locker) {
				data.Clear();
			}
		}

		void Add(CookieData cookie)
		{
			string domainName = cookie.domain;

			if(!data.ContainsKey(domainName)) {
				data.Add(domainName, new List<CookieData>());
			}
			CookieData target = data[domainName].Find(c => c.name == cookie.name);
			if(target != null) {
				data[domainName].Remove(target);
			}
			if(cookie.expires == null || cookie.expires >= DateTime.Now) {
				data[domainName].Add(cookie);
			}
		}

		List<CookieData> Fetch(Uri uri, string key)
		{
			var relevants = new List<CookieData>();
			bool isSsl = uri.Scheme == Uri.UriSchemeHttps;
			if(data.ContainsKey(key)) {
				data[key].ForEach(c => {
					if(c.IsExpired) {
						return;
					}
					if(c.secure && !isSsl) {
						return;
					}
					if(c.ExactMatchOnly && uri.Host != key) {
						return;
					}
					// add to qualification only if the path matches
					if(uri.AbsolutePath.IndexOf(c.path) == 0) {
						relevants.Add(c);
					}
				});
			}
			return relevants;
		}

		internal void SaveToFile()
		{
			lock(locker) {
				CleanUp();
				var saveable = new Dictionary<string, List<CookieData>>();
				foreach(string key in data.Keys) {
					saveable.Add(key, data[key].FindAll(c => !c.IsSession));
				}
				io.Write(saveable);
			}
		}

		internal Dictionary<string, List<CookieData>> ReadFromFile()
		{
			if(!io.Exists) {
				return new Dictionary<string, List<CookieData>>();
			}
			try {
				return io.Read<Dictionary<string, List<CookieData>>>();
			}
			catch(IOException) {
				return new Dictionary<string, List<CookieData>>();
			}
		}

		public override string ToString()
		{
			lock(locker) {
				StringBuilder sb = new StringBuilder();
				foreach(string key in data.Keys) {
					sb.Append(key + ":\n");
					foreach(var cookie in data[key]) {
						sb.Append("   " + cookie.ToString() + "\n");
					}
				}
				return sb.ToString();
			}
		}
	}
}
