using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniHttp
{
	public class DefaultFileHandler : IFileHandler
	{
		public bool Exists(string path)
		{
			return File.Exists(path);
		}

		public virtual FileStream OpenWriteStream(string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		public virtual void Write(string path, byte[] data)
		{
			FileInfo info = new FileInfo(path);
			FileInfo temp = new FileInfo(info.FullName + ".tmp");
			info.Directory.Create();
			temp.Delete();
			File.WriteAllBytes(temp.FullName, data);
			info.Delete();
			File.Move(temp.FullName, info.FullName);
		}

		public void WriteObject<T>(string path, T obj) where T : class
		{
			MemoryStream stream = new MemoryStream();
			new BinaryFormatter().Serialize(stream, obj);
			Write(path, stream.ToArray());
		}

		public virtual FileStream OpenReadStream(string path)
		{
			return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public virtual byte[] Read(string path)
		{
			return File.ReadAllBytes(path);
		}

		public T ReadObject<T>(string path) where T : class
		{
			MemoryStream stream = new MemoryStream(Read(path));
			return new BinaryFormatter().Deserialize(stream) as T;
		}
	}
}
