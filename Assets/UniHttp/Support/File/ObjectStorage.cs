using UnityEngine;
using System.IO;

namespace UniHttp
{
	public class ObjectStorage
	{
		public IFileHandler fileHandler;

		public string path;

		public ObjectStorage(IFileHandler fileHandler, string path)
		{
			this.fileHandler = fileHandler;
			this.path = path;
		}

		public bool Exists
		{
			get { return File.Exists(path); }
		}

		public T Read<T>() where T : class
		{
			return fileHandler.ReadObject<T>(path);
		}

		public void Write<T>(T obj) where T : class
		{
			fileHandler.WriteObject(path, obj);
		}
	}
}
