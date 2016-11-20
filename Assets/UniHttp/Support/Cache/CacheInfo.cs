using UnityEngine;
using System;
using System.Text;

namespace UniHttp
{
	[Serializable()]
	internal sealed class CacheInfo
	{
		internal string domain;
		internal string path;
		internal int fileSize;
		internal string eTag;
		internal DateTime? expireAt;
		internal DateTime? lastModified;
		internal DateTime createdAt;

		internal CacheInfo(HttpResponse response)
		{
			this.createdAt = DateTime.Now;
			Update(response);
		}

		internal void Update(HttpResponse response)
		{
			Uri uri = response.Request.Uri;

			this.domain = uri.Host;
			this.path = uri.AbsolutePath;
			this.fileSize = response.MessageBody.Length;

			if(response.Headers.Exist("ETag")) {
				this.eTag = response.Headers["ETag"][0];
			}
			if(response.Headers.Exist("Expires")) {
				this.expireAt = DateTime.Parse(response.Headers["Expires"][0]);
			}
			if(response.Headers.Exist("Last-Modified")) {
				this.lastModified = DateTime.Parse(response.Headers["Last-Modified"][0]);
			}
			if(response.Headers.Exist("Cache-Control") && response.Headers["Cache-Control"][0].Contains("max-age")) {
				foreach(string directive in response.Headers["Cache-Control"][0].Split(',')) {
					if(directive.Contains("max-age")) {
						int maxAge = int.Parse(directive.Split('=')[1]);
						this.expireAt = DateTime.Now + TimeSpan.FromSeconds(maxAge);
					}
				}
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[Cache] ");
			sb.AppendFormat("{0}{1} (size: {2:0.00}KB)\n", domain, path, fileSize);
			if(eTag != null) {
				sb.AppendFormat("ETag={0}  ", eTag);
			}
			if(expireAt.HasValue) {
				sb.AppendFormat("ExpiresAt={0}  ", expireAt);
			}
			if(lastModified.HasValue) {
				sb.AppendFormat("Last-Modified={0}  ", lastModified);
			}
			return sb.ToString();
		}
	}
}
