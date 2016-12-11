using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace UniHttp
{
	[Serializable()]
	internal sealed class CacheInfo
	{
		internal string domain;
		internal string path;
		internal List<string> contentType;
		internal string eTag;
		internal DateTimeOffset? expireAt;
		internal DateTimeOffset? lastModified;
		internal DateTimeOffset createdAt;

		internal CacheInfo(HttpResponse response)
		{
			this.createdAt = DateTimeOffset.Now;
			Update(response);
		}

		internal void Update(HttpResponse response)
		{
			Uri uri = response.Request.Uri;

			this.domain = uri.Authority;
			this.path = uri.AbsolutePath;
			if(response.Headers.Exist("Content-Type")) {
				this.contentType = response.Headers["Content-Type"];
			}
			if(response.Headers.Exist("ETag")) {
				this.eTag = response.Headers["ETag"][0];
			}
			if(response.Headers.Exist("Expires")) {
				this.expireAt = DateTimeOffset.Parse(response.Headers["Expires"][0]);
			}
			if(response.Headers.Exist("Last-Modified")) {
				this.lastModified = DateTimeOffset.Parse(response.Headers["Last-Modified"][0]);
			}
			if(response.Headers.Exist("Cache-Control") && response.Headers["Cache-Control"][0].Contains("max-age")) {
				foreach(string directive in response.Headers["Cache-Control"][0].Split(',')) {
					if(directive.Contains("max-age")) {
						int maxAge = int.Parse(directive.Split('=')[1]);
						this.expireAt = DateTimeOffset.Now + TimeSpan.FromSeconds(maxAge);
					}
				}
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[Cache] ");
			sb.AppendFormat("{0}{1}\n", domain, path);
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
