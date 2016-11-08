using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UniHttp
{
	public sealed class DefaultCacheStorage : ICacheStorage
	{
		string salt;
		MD5 hash;

		public DefaultCacheStorage(string salt)
		{
			this.salt = salt;
			this.hash = new MD5CryptoServiceProvider();
		}

		public void Write(DirectoryInfo baseDirectory, Uri uri, byte[] data)
		{
			string filename = ComputeFileName(uri);
			FileInfo info = new FileInfo(baseDirectory.FullName + filename);
			FileInfo temp = new FileInfo(info.FullName + ".tmp");
			temp.Delete();
			File.WriteAllBytes(temp.FullName, data);
			info.Delete();
			File.Move(temp.FullName, info.FullName);
		}

		public byte[] Read(Uri uri)
		{
			return File.ReadAllBytes(ComputeFileName(uri));
		}

		string ComputeFileName(Uri uri)
		{
			string data = string.Concat(salt, uri.Authority, uri.AbsolutePath);
			byte[] bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(data));
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < bytes.Length; i++) {
				sb.Append(bytes[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
