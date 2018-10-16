using System;
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

		internal string Name { get; private set; }

		internal CookieDomain(string baseDirectory, string domain, IFileHandler fileHandler)
		{
			this.filePath = BuildPath(baseDirectory, domain);
			this.fileHandler = fileHandler;
			this.cookies = ReadFromFile();
			this.Name = domain;
		}

		internal static string BuildPath(string baseDirectory, string domain)
		{
			return string.Concat(
				baseDirectory,
				Path.DirectorySeparatorChar,
				domain,
				".cookies"
			);
		}

		public IEnumerator GetEnumerator()
		{
			return cookies.GetEnumerator();
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

		Cookie FindMatch(Cookie matcher)
		{
			foreach(Cookie cookie in cookies) {
				if(matcher.Name == cookie.Name) {
					return cookie;
				}
			}
			return null;
		}

		internal void WriteToFile()
		{
			cookies.RemoveAll(c => c.IsExpired);

			List<Cookie> targets = cookies.FindAll(cookie => !cookie.IsSession);

			if(targets.Count == 0) {
				return;
			}

			using(Stream stream = fileHandler.OpenWriteStream(filePath)) {
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(targets.Count);
				foreach(Cookie cookie in targets) {
					writer.Write(cookie.Name);
					writer.Write(cookie.Value);
					writer.Write(cookie.Domain != null);
					if(cookie.Domain != null) {
						writer.Write(cookie.Domain);
					}
					writer.Write(cookie.Path);
					writer.Write(cookie.Expires.HasValue);
					if(cookie.Expires.HasValue) {
						writer.Write(cookie.Expires.Value.ToBinary());
					}
					writer.Write(cookie.Secure);
					writer.Write(cookie.HttpOnly);
					writer.Write(cookie.Size);
				}
			}
		}

		List<Cookie> ReadFromFile()
		{
			if(!fileHandler.Exists(filePath)) {
				return new List<Cookie>();
			}
			try {
				using(Stream stream = fileHandler.OpenReadStream(filePath)) {
					BinaryReader reader = new BinaryReader(stream);
					int size = reader.ReadInt32();
					List<Cookie> readCookies = new List<Cookie>(size);
					for(int i = 0; i < size; i++) {
						readCookies.Add(new Cookie {
							Name = reader.ReadString(),
							Value = reader.ReadString(),
							Domain = reader.ReadBoolean() ? reader.ReadString() : null,
							Path = reader.ReadString(),
							Expires = reader.ReadBoolean() ? (DateTime?)DateTime.FromBinary(reader.ReadInt64()) : null,
							Secure = reader.ReadBoolean(),
							HttpOnly = reader.ReadBoolean(),
							Size = reader.ReadInt32(),
						});
					}
					return readCookies;
				}
			}
			catch(IOException exception) {
				Debug.LogWarning(exception);
				return new List<Cookie>();
			}
		}
	}
}
