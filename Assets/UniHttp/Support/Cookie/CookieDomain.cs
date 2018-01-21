using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace UniHttp
{
	internal class CookieDomain : IEnumerable
	{
		readonly IFileHandler fileHandler;
		readonly string filePath;
		readonly CookieParser parser;
		readonly List<Cookie> cookies;
		internal readonly string name;

		internal CookieDomain(string baseDirectory, string domain, CookieParser parser, IFileHandler fileHandler) {
			this.filePath = BuildPath(baseDirectory, domain);
			this.fileHandler = fileHandler;
			this.parser = parser;
			this.cookies = ReadFromFile();
			this.name = domain;
		}

		internal static string BuildPath(string baseDirectory, string domain)
		{
			return string.Concat(
				baseDirectory,
				Path.DirectorySeparatorChar,
				domain,
				".cookies.txt"
			);
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

			StringBuilder sb = new StringBuilder();
			foreach(Cookie cookie in cookies) {
				if(cookie.IsSession) {
					continue;
				}
				sb.Append(cookie.original);
				sb.Append("\n");
			}
			byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
			fileHandler.Write(filePath, bytes);
		}

		List<Cookie> ReadFromFile()
		{
			if(!fileHandler.Exists(filePath)) {
				return new List<Cookie>();
			}
			try {
				return parser.Parse(fileHandler.Read(filePath));
			}
			catch(IOException exception) {
				Debug.LogWarning(exception);
				return new List<Cookie>();
			}
			catch(SerializationException exception) {
				Debug.LogWarning(exception);
				return new List<Cookie>();
			}
		}
	}
}
