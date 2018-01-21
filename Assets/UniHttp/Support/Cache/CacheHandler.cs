using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniHttp
{
	internal sealed class CacheHandler
	{
		readonly BinaryFormatter formatter;
		readonly IFileHandler fileHandler;
		readonly DirectoryInfo baseDirectory;

		internal CacheHandler(IFileHandler fileHandler, string dataDirectory)
		{
			this.formatter = new BinaryFormatter();
			this.fileHandler = fileHandler;
			this.baseDirectory = new DirectoryInfo(dataDirectory).CreateSubdirectory("Cache");
		}

		internal bool IsCachable(HttpRequest request)
		{
			if(request.Method != HttpMethod.GET && request.Method != HttpMethod.HEAD) {
				return false;
			}
			if(request.Headers.Exist(HeaderField.CacheControl, "no-store")) {
				return false;
			}
			if(!string.IsNullOrEmpty(request.Uri.Query)) {
				return false;
			}
			if(request.Data != null) {
				return false;
			}
			return true;
		}

		internal bool IsCachable(HttpResponse response)
		{
			if(!IsCachable(response.Request)) {
				return false;
			}
			if(response.StatusCode != StatusCode.OK) {
				return false;
			}
			if(response.MessageBody.Length == 0) {
				return false;
			}
			if(response.Headers.Exist(HeaderField.ETag)) {
				return true;
			}
			if(response.Headers.Exist(HeaderField.LastModified)) {
				return true;
			}
			if(response.Headers.Exist(HeaderField.Expires)) {
				return true;
			}
			if(response.Headers.Exist(HeaderField.CacheControl) && response.Headers[HeaderField.CacheControl][0].Contains("max-age")) {
				return true;
			}
			return false;
		}

		internal CacheMetadata FindMetadata(HttpRequest request)
		{
			string metaPath = GetMetaPath(request.Uri);
			CacheMetadata data = null;

			Mutex mutex = new Mutex(false, metaPath);
			mutex.WaitOne();
			try {
				if(fileHandler.Exists(metaPath)) {
					data = ReadFromFile(metaPath);
				}
			}
			finally {
				mutex.ReleaseMutex();
			}

			return data;
		}

		internal CacheStream GetDataReadStream(HttpRequest request)
		{
			string dataPath = GetDataPath(request.Uri);
			Mutex mutex = new Mutex(false, dataPath);
			mutex.WaitOne();
			try {
				Stream stream = fileHandler.OpenReadStream(dataPath);
				return new CacheStream(stream, mutex);
			}
			catch(Exception exception) {
				mutex.ReleaseMutex();
				throw exception;
			}
		}

		internal void CacheResponse(HttpResponse response)
		{
			string metaPath = GetMetaPath(response.Request.Uri);
			string dataPath = GetDataPath(response.Request.Uri);

			Mutex indexMutex = new Mutex(false, metaPath);
			Mutex dataMutex = new Mutex(false, dataPath);
			indexMutex.WaitOne();
			dataMutex.WaitOne();
			try {
				WriteToFile(dataPath, response);
				WriteToFile(metaPath, new CacheMetadata(response));
			}
			finally {
				indexMutex.ReleaseMutex();
				dataMutex.ReleaseMutex();
			}
		}

		internal void ClearAll()
		{
			foreach(DirectoryInfo dir in baseDirectory.GetDirectories()) {
				foreach(FileInfo file in dir.GetFiles()) {
					Mutex mutex = new Mutex(false, file.FullName);
					mutex.WaitOne();
					try {
						fileHandler.Delete(file.FullName);
					}
					finally {
						mutex.ReleaseMutex();
					}
				}
				dir.Delete(true);
			}
		}

		string GetMetaPath(Uri uri)
		{
			return GetDataPath(uri) + ".meta";
		}

		string GetDataPath(Uri uri)
		{
			return GetBasePath(uri) + ".cache";
		}

		string GetBasePath(Uri uri)
		{
			return string.Concat(
				baseDirectory.FullName,
				Path.DirectorySeparatorChar,
				uri.Authority.Replace(":", "_"),
			    Path.DirectorySeparatorChar,
				uri.AbsolutePath
			);
		}

		public CacheMetadata ReadFromFile(string path)
		{
			MemoryStream stream = new MemoryStream(fileHandler.Read(path));
			return formatter.Deserialize(stream) as CacheMetadata;
		}

		public void WriteToFile(string path, HttpResponse response)
		{
			fileHandler.Write(path, response.MessageBody);
		}

		public void WriteToFile(string path, CacheMetadata metadata)
		{
			MemoryStream stream = new MemoryStream();
			formatter.Serialize(stream, metadata);
			fileHandler.Write(path, stream.ToArray());
		}
	}
}
