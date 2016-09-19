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
	internal class CookieJar
	{
		string filePath;
		Dictionary<string, List<Cookie>> cookies;

		internal CookieJar(string filePath)
		{
			this.filePath = filePath;
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

		internal void AddRange(List<Cookie> cookies)
		{
			Monitor.Enter(cookies);
			cookies.ForEach(Add);
			Monitor.Exit(cookies);
		}

		internal void Clear()
		{
			Monitor.Enter(cookies);
			cookies.Clear();
			Monitor.Exit(cookies);
		}

		void Add(Cookie cookie)
		{
			string key = cookie.Domain;

			if(!cookies.ContainsKey(key)) {
				cookies.Add(key, new List<Cookie>());
			}
			Cookie target = cookies[key].Find(c => c.Name == cookie.Name);
			if(target != null) {
				cookies[key].Remove(target);
			}
			if(cookie.Expires == null || cookie.Expires >= DateTime.Now) {
				cookies[key].Add(cookie);
			}
		}

		List<Cookie> Fetch(Uri uri, string key)
		{
			var relevants = new List<Cookie>();
			var now = DateTime.Now;

			if(cookies.ContainsKey(key)) {
				cookies[key].ForEach(c => {
					if(c.Expires.HasValue && c.Expires < now) {
						return;
					}

					if(c.ExactMatchOnly && uri.Host != key) {
						return;
					}
					// add to qualification only if the path matches
					if(uri.AbsolutePath.IndexOf(c.Path) == 0) {
						relevants.Add(c);
					}
				});
			}
			return relevants;
		}

		public void SaveToFile()
		{
			using(Stream stream = File.Open(filePath, FileMode.Create)) {
				new BinaryFormatter().Serialize(stream, cookies);
			}
		}

		public Dictionary<string, List<Cookie>> ReadFromFile()
		{
			if(File.Exists(filePath)) {
				using(Stream stream = File.OpenRead(filePath)) {
					var binaryFormatter = new BinaryFormatter();
					return binaryFormatter.Deserialize(stream) as Dictionary<string, List<Cookie>>;
				}
			} else {
				return new Dictionary<string, List<Cookie>>();
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
