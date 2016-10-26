using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace UniHttp
{
	internal sealed class CacheHandler
	{
		DirectoryInfo baseDirectory;
		Dictionary<string, Cache> caches;
		object locker;

		internal CacheHandler(DirectoryInfo baseDirectory)
		{
			this.baseDirectory = baseDirectory;
			this.caches = new Dictionary<string, Cache>();
			this.locker = new object();
		}

		internal bool IsCachable(HttpRequest request)
		{
			if(request.Method != HttpMethod.GET || request.Method != HttpMethod.HEAD) {
				return false;
			}
			if(request.Headers.Exist("Cache-Control", "no-store")) {
				return false;
			}
			if(!string.IsNullOrEmpty(request.Uri.Query)) {
				return false;
			}
			if(request.Payload != null) {
				return false;
			}
			return true;
		}

		internal bool IsCachable(HttpResponse response)
		{
			if(!IsCachable(response.Request)) {
				return false;
			}
			if(response.StatusCode != 200) {
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

		internal Cache Find(HttpRequest request)
		{
			if(!IsCachable(request)) {
				return null;
			}
			if(!File.Exists(request.Uri.AbsoluteUri)) {
				return null;
			}
			lock(locker) {
				if(caches.ContainsKey(request.Uri.AbsoluteUri)) {
					return caches[request.Uri.AbsoluteUri];	
				}
			}
			return null;
		}

		internal void CacheResponse(HttpResponse response)
		{
			string url = response.Request.Uri.AbsoluteUri;
			WriteToFile(url, response.MessageBody);
			UpdateDictionary(url, response);
		}

		void WriteToFile(string url, byte[] data)
		{
			string filePath = baseDirectory.FullName + url;
			string tempPath = filePath + ".tmp";
			File.WriteAllBytes(tempPath, data);
			File.Move(tempPath, filePath);
		}

		void UpdateDictionary(string url, HttpResponse response)
		{
			lock(locker) {
				if(caches.ContainsKey(url)) {
					caches[url].Update(response);
				} else {
					caches.Add(url, new Cache(response));
				}
			}
		}

		internal void Clear()
		{
			baseDirectory.Delete(true);
			caches.Clear();
		}
	}
}
