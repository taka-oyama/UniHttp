using UnityEngine;
using System.IO;

namespace UniHttp
{
	public class ObjectFile
	{
		public IFileHandler io;

		public string path;

		public ObjectFile(IFileHandler io, string path)
		{
			this.io = io;
			this.path = path;
		}

		public bool Exists
		{
			get { return File.Exists(path); }
		}

		public T Read<T>() where T : class
		{
			return io.ReadObject<T>(path);
		}

		public void Write<T>(T obj) where T : class
		{
			io.WriteObject(path, obj);
		}
	}
}
