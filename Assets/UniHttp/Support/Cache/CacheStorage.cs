using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UniHttp
{
	public class CacheStorage
	{
		// for some reason File.Exists returns false when threading, so I'm forced to lock it.
		object locker;

		IFileHandler fileHandler;
		DirectoryInfo baseDirectory;
		MD5 hash;
		string password;

		public CacheStorage(IFileHandler fileHandler, DirectoryInfo baseDirectory)
		{
			this.locker = new object();
			this.fileHandler = fileHandler;
			this.baseDirectory = baseDirectory;
			this.hash = new MD5CryptoServiceProvider();
			this.password = Application.identifier;
		}

		public virtual void Write(Uri uri, byte[] data)
		{
			lock(locker) {
				fileHandler.Write(ComputeDirectory(uri) + ComputeFileName(uri), data);
			}
		}

		public virtual byte[] Read(Uri uri)
		{
			lock(locker) {
				return fileHandler.Read(ComputeDirectory(uri) + ComputeFileName(uri));
			}
		}

		public virtual bool Exists(Uri uri)
		{
			lock(locker) {
				return fileHandler.Exists(ComputeDirectory(uri) + ComputeFileName(uri));
			}
		}

		public virtual void Clear()
		{
			lock(locker) {
				var dirs = baseDirectory.GetDirectories();
				for(var i = 0; i < dirs.Length; i++) {
					dirs[i].Delete(true);
				}
			}
		}

		string ComputeDirectory(Uri uri)
		{
			return baseDirectory.FullName + uri.Authority.Replace(":", "_") + "/";
		}

		string ComputeFileName(Uri uri)
		{
			string data = string.Concat(password, uri.Authority, uri.AbsolutePath);
			byte[] bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(data));
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < bytes.Length; i++) {
				sb.Append(bytes[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
