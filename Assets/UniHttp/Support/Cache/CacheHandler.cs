using UnityEngine;
using System;
using System.IO;
using System.Threading;

namespace UniHttp
{
	internal sealed class CacheHandler
	{
		readonly IFileHandler fileHandler;
		readonly DirectoryInfo baseDirectory;

		internal CacheHandler(IFileHandler fileHandler, string dataDirectory)
		{
			this.fileHandler = fileHandler;
			this.baseDirectory = new DirectoryInfo(dataDirectory).CreateSubdirectory("Cache");
		}

		internal bool IsCachable(HttpRequest request)
		{
			if(request.Method != HttpMethod.GET && request.Method != HttpMethod.HEAD) {
				return false;
			}
			if(request.Headers.Contains(HeaderField.CacheControl, "no-store")) {
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
			if(response.Headers.Contains(HeaderField.ETag)) {
				return true;
			}
			if(response.Headers.Contains(HeaderField.LastModified)) {
				return true;
			}
			if(response.Headers.Contains(HeaderField.Expires)) {
				return true;
			}
			if(response.Headers.Contains(HeaderField.CacheControl) && response.Headers[HeaderField.CacheControl][0].Contains("max-age")) {
				return true;
			}
			return false;
		}

		internal CacheMetadata FindMetadata(HttpRequest request)
		{
			string filePath = GetFilePath(request.Uri);
			CacheMetadata data = null;
			Mutex mutex = new Mutex(false, filePath);
			mutex.WaitOne();
			try {
				if(fileHandler.Exists(filePath)) {
					data = ReadMetaData(filePath);
				}
			}
			finally {
				mutex.ReleaseMutex();
			}
			return data;
		}

		internal CacheStream GetMessageBodyStream(HttpRequest request)
		{
			string filePath = GetFilePath(request.Uri);
			Mutex mutex = new Mutex(false, filePath);
			Stream stream = null;
			mutex.WaitOne();
			try {
				stream = fileHandler.OpenReadStream(filePath);
				SkipMetaData(stream);
				return new CacheStream(stream, mutex);
			}
			catch(Exception exception) {
				if(stream != null) {
					stream.Dispose();
				}
				mutex.ReleaseMutex();
				throw exception;
			}
		}

		internal void CacheResponse(HttpResponse response)
		{
			string filePath = GetFilePath(response.Request.Uri);
			Mutex mutex = new Mutex(false, filePath);
			mutex.WaitOne();
			try {
				WriteToFile(filePath, response);
			}
			finally {
				mutex.ReleaseMutex();
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

		CacheMetadata ReadMetaData(string path)
		{
			using(Stream stream = fileHandler.OpenReadStream(path)) {
				using(BinaryReader reader = new BinaryReader(stream)) {
					int fileVersion = reader.ReadInt32();
					if(fileVersion != CacheMetadata.version) {
						return null;
					}
					return new CacheMetadata {
						domain = reader.ReadBoolean() ? reader.ReadString() : null,
						path = reader.ReadBoolean() ? reader.ReadString() : null,
						contentType = reader.ReadBoolean() ? reader.ReadString() : null,
						eTag = reader.ReadBoolean() ? reader.ReadString() : null,
						expireAt = reader.ReadBoolean() ? (DateTime?)DateTime.FromBinary(reader.ReadInt64()) : null,
						lastModified = reader.ReadBoolean() ? (DateTime?)DateTime.FromBinary(reader.ReadInt64()) : null
					};
				}
			}
		}

		void SkipMetaData(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			reader.ReadInt32();
			if(reader.ReadBoolean()) reader.ReadString();
			if(reader.ReadBoolean()) reader.ReadString();
			if(reader.ReadBoolean()) reader.ReadString();
			if(reader.ReadBoolean()) reader.ReadString();
			if(reader.ReadBoolean()) reader.ReadInt64();
			if(reader.ReadBoolean()) reader.ReadInt64();
		}

		void WriteToFile(string path, HttpResponse response)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));

			using(Stream stream = fileHandler.OpenWriteStream(path)) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					CacheMetadata meta = new CacheMetadata(response);
					writer.Write(CacheMetadata.version);
					writer.Write(meta.domain != null);
					writer.Write(meta.domain);
					writer.Write(meta.path != null);
					writer.Write(meta.path);
					writer.Write(meta.contentType != null);
					writer.Write(meta.contentType);
					writer.Write(meta.eTag != null);
					writer.Write(meta.eTag);
					writer.Write(meta.expireAt.HasValue);
					if(meta.expireAt.HasValue) {
						writer.Write(meta.expireAt.Value.ToBinary());
					}
					writer.Write(meta.lastModified.HasValue);
					if(meta.lastModified.HasValue) {
						writer.Write(meta.lastModified.Value.ToBinary());
					}
					writer.Write(response.MessageBody);
				}
			}
		}

		string GetFilePath(Uri uri)
		{
			return string.Concat(
				baseDirectory.FullName,
				Path.DirectorySeparatorChar,
				uri.Authority.Replace(":", "_"),
			    Path.DirectorySeparatorChar,
				uri.AbsolutePath,
				".cache"
			);
		}
	}
}
