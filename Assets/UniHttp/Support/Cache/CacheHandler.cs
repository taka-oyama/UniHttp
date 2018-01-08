﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace UniHttp
{
	internal sealed class CacheHandler
	{
		readonly object locker;
		readonly FileStore infoStore;
		Dictionary<string, CacheData> caches;
		CacheStore cacheStorage;

		internal CacheHandler(IFileHandler fileHandler, string dataDirectory)
		{
			this.locker = new object();
			this.infoStore = new FileStore(fileHandler, dataDirectory + "/CacheInfo.bin");
			this.cacheStorage = new CacheStore(fileHandler, dataDirectory);
			this.caches = ReadFromFile();
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

		internal CacheData Find(HttpRequest request)
		{
			CacheData cache = null;

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

		internal CacheData CacheResponse(HttpResponse response)
		{
			string url = response.Request.Uri.AbsoluteUri;

			lock(locker) {
				if(caches.ContainsKey(url)) {
					caches[url].Update(response);
				} else {
					caches.Add(url, new CacheData(response));
				}
				cacheStorage.Write(response.Request.Uri, response.MessageBody);
				return caches[url];
			}
		}

		internal CacheStream GetReadStream(HttpRequest request)
		{
			return new CacheStream(cacheStorage.OpenReadStream(request.Uri));
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
				infoStore.Write(caches);
			}
		}

		Dictionary<string, CacheData> ReadFromFile()
		{
			if(!infoStore.Exists) {
				return new Dictionary<string, CacheData>();
			}

			try {
				return infoStore.Read<Dictionary<string, CacheData>>();
			}
			catch(IOException e) {
				Debug.LogWarning(e);
				return new Dictionary<string, CacheData>();
			}
			catch(SerializationException e) {
				Debug.LogWarning(e);
				return new Dictionary<string, CacheData>();
			}
		}
	}
}
