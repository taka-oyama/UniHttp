using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace UniHttp
{
	public class SecureFileIO
	{
		FileInfo info;
		string password;

		public bool Exists { get { return info.Exists; } }

		public SecureFileIO(string path, string password) : this(new FileInfo(path), password) {}

		public SecureFileIO(FileInfo info, string password)
		{
			this.info = info;
			this.password = password;
		}

		public void Write(byte[] data)
		{
			info.Directory.Create();
			FileInfo temp = new FileInfo(info.FullName + ".tmp");
			temp.Delete();
			File.WriteAllBytes(temp.FullName, BeforeWrite(data));
			info.Delete();
			File.Move(temp.FullName, info.FullName);
		}

		public void WriteObject<T>(T obj) where T : class
		{
			using(var stream = new MemoryStream()) {
				new BinaryFormatter().Serialize(stream, obj);
				Write(stream.ToArray());
			}
		}

		public byte[] Read()
		{
			byte[] readBytes = File.ReadAllBytes(info.FullName);
			return AfterRead(readBytes);
		}

		public T ReadObject<T>() where T : class
		{
			using(var stream = new MemoryStream(Read())) {
				return new BinaryFormatter().Deserialize(stream) as T;
			}
		}

		byte[] AfterRead(byte[] bytes)
		{
			return AuthenticatedEncryption.Decrypt(password, bytes);
		}

		byte[] BeforeWrite(byte[] bytes)
		{
			return AuthenticatedEncryption.Encrypt(password, bytes);
		}
	}
}
