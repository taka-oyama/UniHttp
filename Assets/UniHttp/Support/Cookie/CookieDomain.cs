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
		internal readonly string name;

		internal CookieDomain(string baseDirectory, string domain, IFileHandler fileHandler)
		{
			this.filePath = BuildPath(baseDirectory, domain);
			this.fileHandler = fileHandler;
			this.cookies = ReadFromFile();
			this.name = domain;
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

		internal void WriteToFile()
		{
			List<Cookie> targets = cookies.FindAll(cookie => {
				return !cookie.IsExpired && !cookie.IsSession;
			});
			if(targets.Count == 0) {
				return;
			}
			using(Stream stream = fileHandler.OpenWriteStream(filePath)) {
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(targets.Count);
				foreach(Cookie cookie in targets) {
					writer.Write(cookie.name);
					writer.Write(cookie.value);
					writer.Write(cookie.domain != null);
					if(cookie.domain != null) {
						writer.Write(cookie.domain);
					}
					writer.Write(cookie.path);
					writer.Write(cookie.expires.HasValue);
					if(cookie.expires.HasValue) {
						writer.Write(cookie.expires.Value.ToBinary());
					}
					writer.Write(cookie.secure);
					writer.Write(cookie.httpOnly);
					writer.Write(cookie.size);
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
							name = reader.ReadString(),
							value = reader.ReadString(),
							domain = reader.ReadBoolean() ? reader.ReadString() : null,
							path = reader.ReadString(),
							expires = reader.ReadBoolean() ? (DateTime?)DateTime.FromBinary(reader.ReadInt64()) : null,
							secure = reader.ReadBoolean(),
							httpOnly = reader.ReadBoolean(),
							size = reader.ReadInt32(),
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
