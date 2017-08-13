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
		CacheStorage storage;

		internal CacheHandler(ObjectStorage infoStorage, CacheStorage cacheStorage)
		{
			this.locker = new object();
			this.infoStorage = infoStorage;
			this.caches = ReadFromFile();
			this.storage = cacheStorage;
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
			if(response.StatusCode != 200 && response.StatusCode != 304) {
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
			if(!IsCachable(request)) {
				return null;
			}
			if(!storage.Exists(request.Uri)) {
				return null;
			}
			lock(locker) {
				if(caches.ContainsKey(request.Uri.AbsoluteUri)) {
					return caches[request.Uri.AbsoluteUri];	
				}
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
				storage.Write(response.Request.Uri, response.MessageBody);
				return caches[url];
			}
		}

		internal void Clear()
		{
			lock(locker) {
				caches.Clear();
				storage.Clear();
			}
		}

		internal void SaveToFile()
		{
			lock(locker) {
				infoStorage.Write(caches);
			}
		}

		internal Dictionary<string, CacheInfo> ReadFromFile()
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
