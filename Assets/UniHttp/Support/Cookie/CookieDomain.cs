using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniHttp
{
	internal class CookieDomain : IEnumerable
	{
		readonly IFileHandler fileHandler;
		readonly string filePath;
		readonly List<Cookie> cookies;
		internal readonly string name;

		internal CookieDomain(HttpResponse response, IFileHandler fileHandler, string baseDirectory)
		{
			this.filePath = baseDirectory + "/" + response.Request.Uri.Host + ".bin";
			this.fileHandler = fileHandler;
			this.cookies = ReadFromFile();
			this.name = response.Request.Uri.Host;
		}

		public IEnumerator GetEnumerator()
		{
			return cookies.GetEnumerator();
		}

		internal Cookie FindMatch(Cookie matcher)
		{
			foreach(Cookie cookie in cookies) {
				if(matcher.name == cookie.name) {
					return cookie;
				}
			}
			return null;
		}

		internal void AddOrReplace(Cookie cookie)
		{
			Cookie target = FindMatch(cookie);
			if(target != null) {
				cookies.Remove(target);
			}
			if(!cookie.IsExpired) {
				cookies.Add(cookie);
			}
		}

		internal void RemoveExpiredCookies()
		{
			cookies.RemoveAll(c => c.IsExpired);
		}

		internal void SaveToFile()
		{
			RemoveExpiredCookies();
			fileHandler.WriteObject(filePath, cookies.FindAll(c => !c.IsSession));
		}

		List<Cookie> ReadFromFile()
		{
			if(!fileHandler.Exists(filePath)) {
				return new List<Cookie>();
			}
			try {
				return fileHandler.ReadObject<List<Cookie>>(filePath);
			}
			catch(IOException) {
				return new List<Cookie>();
			}
		}
	}
}
