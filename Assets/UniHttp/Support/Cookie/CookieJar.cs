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
		Dictionary<string, List<Cookie>> cookies;

		internal CookieJar(ObjectStorage io)
		{
			this.locker = new object();
			this.io = io;
			this.cookies = ReadFromFile();
		}

		internal List<Cookie> FindMatch(Uri uri)
		{
			List<Cookie> relevants = new List<Cookie>();
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

		internal void AddOrReplaceRange(List<Cookie> cookies)
		{
			lock(locker) {
				cookies.ForEach(Add);
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

		internal void SaveToFile()
		{
			lock(locker) {
				CleanUp();
				var saveable = new Dictionary<string, List<Cookie>>();
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
			lock(locker) {
				return io.Read<Dictionary<string, List<Cookie>>>();
			}
		}

		public override string ToString()
		{
			lock(locker) {
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
}
