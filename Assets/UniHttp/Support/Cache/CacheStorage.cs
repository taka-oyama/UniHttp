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
		readonly object locker;
		readonly IFileHandler fileHandler;
		readonly DirectoryInfo baseDirectory;
		readonly MD5 hash;
		readonly string password;

		public CacheStorage(IFileHandler fileHandler, string baseDirectory)
		{
			this.locker = new object();
			this.fileHandler = fileHandler;
			this.baseDirectory = new DirectoryInfo(baseDirectory).CreateSubdirectory("Cache");
			this.hash = new MD5CryptoServiceProvider();
			this.password = Application.identifier;
		}

		public virtual void Write(Uri uri, byte[] data)
		{
			lock(locker) {
				fileHandler.Write(ComputePath(uri), data);
			}
		}

		public virtual byte[] Read(Uri uri)
		{
			lock(locker) {
				return fileHandler.Read(ComputePath(uri));
			}
		}

		public FileStream OpenReadStream(Uri uri)
		{
			lock(locker) {
				return fileHandler.OpenReadStream(ComputePath(uri));
			}
		}

		public virtual bool Exists(Uri uri)
		{
			lock(locker) {
				return fileHandler.Exists(ComputePath(uri));
			}
		}

		public virtual void Clear()
		{
			lock(locker) {
				DirectoryInfo[] dirs = baseDirectory.GetDirectories();
				for(int i = 0; i < dirs.Length; i++) {
					dirs[i].Delete(true);
				}
			}
		}

		string ComputePath(Uri uri)
		{
			return ComputeDirectory(uri) + ComputeFileName(uri);
		}

		string ComputeDirectory(Uri uri)
		{
			return baseDirectory.FullName + "/" + uri.Authority.Replace(":", "_") + "/";
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
