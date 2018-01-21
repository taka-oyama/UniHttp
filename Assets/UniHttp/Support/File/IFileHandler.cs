﻿using System.IO;

namespace UniHttp
{
	public interface IFileHandler
	{
		bool Exists(string path);

		byte[] Read(string path);

		void Write(string path, byte[] data);

		void Delete(string path);

		FileStream OpenReadStream(string path);

		FileStream OpenWriteStream(string path);

		T ReadObject<T>(string path) where T : class;

		void WriteObject<T>(string path, T obj) where T : class;
	}
}
