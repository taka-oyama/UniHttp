using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniHttp
{
	internal sealed class CookieJar
	{
		FileInfo storage;
		Dictionary<string, List<Cookie>> cookies;

		internal CookieJar(FileInfo storage)
		{
			this.storage = storage;
			this.cookies = ReadFromFile();
		}

		internal List<Cookie> FindMatch(Uri uri)
		{
			List<Cookie> relevants = new List<Cookie>();
			IPAddress address;

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

		internal void AddOrReplaceRange(List<Cookie> cookies)
		{
			Monitor.Enter(cookies);
			cookies.ForEach(Add);
			Monitor.Exit(cookies);
		}

		internal void CleanUp()
		{
			foreach(string key in cookies.Keys) {
				cookies[key].RemoveAll(c => c.IsExpired);
			}
		}

		internal void Clear()
		{
			Monitor.Enter(cookies);
			cookies.Clear();
			Monitor.Exit(cookies);
		}

		void Add(Cookie cookie)
		{
			string key = cookie.domain;

			if(!cookies.ContainsKey(key)) {
				cookies.Add(key, new List<Cookie>());
			}
			Cookie target = cookies[key].Find(c => c.name == cookie.name);
			if(target != null) {
				cookies[key].Remove(target);
			}
			if(cookie.expires == null || cookie.expires >= DateTime.Now) {
				cookies[key].Add(cookie);
			}
		}

		List<Cookie> Fetch(Uri uri, string key)
		{
			var relevants = new List<Cookie>();
			bool isSsl = uri.Scheme == Uri.UriSchemeHttps;
			if(cookies.ContainsKey(key)) {
				cookies[key].ForEach(c => {
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

		public void SaveToFile()
		{
			CleanUp();
			var saveable = new Dictionary<string, List<Cookie>>();
			foreach(string key in cookies.Keys) {
				saveable.Add(key, cookies[key].FindAll(c => !c.IsSession));
			}
			using(Stream stream = storage.Create()) {
				new BinaryFormatter().Serialize(stream, saveable);
			}
		}

		public Dictionary<string, List<Cookie>> ReadFromFile()
		{
			if(!storage.Exists) {
				return new Dictionary<string, List<Cookie>>();
			}
			using(Stream stream = storage.OpenRead()) {
				var binaryFormatter = new BinaryFormatter();
				return binaryFormatter.Deserialize(stream) as Dictionary<string, List<Cookie>>;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(string key in cookies.Keys) {
				sb.Append(key + ":\n");
				foreach(var cookie in cookies[key]) {
					sb.Append("   " + cookie.ToString() + "\n");
				}
			}
			return sb.ToString();
		}
	}
}
