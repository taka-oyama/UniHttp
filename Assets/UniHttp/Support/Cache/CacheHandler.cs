using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniHttp
{
	internal sealed class CacheHandler
	{
		object locker;
		ObjectStorage infoStorage;
		Dictionary<string, CacheInfo> caches;
		CacheStorage cacheStorage;

		internal CacheHandler(IFileHandler fileHandler, string dataDirectory)
		{
			this.locker = new object();
			this.infoStorage = new ObjectStorage(fileHandler, dataDirectory + "/CacheInfo.bin");
			this.cacheStorage = new CacheStorage(fileHandler, dataDirectory);
			this.caches = ReadFromFile();
		}

		internal bool IsCachable(HttpRequest request)
		{
			if(request.Method != HttpMethod.GET && request.Method != HttpMethod.HEAD) {
				return false;
			}
			if(request.Headers.Exist("Cache-Control", "no-store")) {
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
			if(response.Headers.Exist("ETag")) {
				return true;
			}
			if(response.Headers.Exist("Last-Modified")) {
				return true;
			}
			if(response.Headers.Exist("Expires")) {
				return true;
			}
			if(response.Headers.Exist("Cache-Control") && response.Headers["Cache-Control"][0].Contains("max-age")) {
				return true;
			}
			return false;
		}

		internal CacheInfo Find(HttpRequest request)
		{
			CacheInfo cache = null;

			lock(locker) {
				if(caches.ContainsKey(request.Uri.AbsoluteUri)) {
					cache = caches[request.Uri.AbsoluteUri];	
				}
			}

			if(cache != null && cacheStorage.Exists(request.Uri)) {
				return cache;
			}

			return null;
		}

		internal CacheInfo CacheResponse(HttpResponse response)
		{
			lock(locker) {
				string url = response.Request.Uri.AbsoluteUri;
				if(caches.ContainsKey(url)) {
					caches[url].Update(response);
				} else {
					caches.Add(url, new CacheInfo(response));
				}
				cacheStorage.Write(response.Request.Uri, response.MessageBody);
				return caches[url];
			}
		}

		internal byte[] RetrieveFromCache(HttpRequest request)
		{
			return cacheStorage.Read(request.Uri);
		}

		internal void Clear()
		{
			lock(locker) {
				caches.Clear();
				cacheStorage.Clear();
			}
		}

		internal void SaveToFile()
		{
			lock(locker) {
				infoStorage.Write(caches);
			}
		}

		Dictionary<string, CacheInfo> ReadFromFile()
		{
			if(!infoStorage.Exists) {
				return new Dictionary<string, CacheInfo>();
			}
			lock(locker) {
				return infoStorage.Read<Dictionary<string, CacheInfo>>();
			}
		}
	}
}
