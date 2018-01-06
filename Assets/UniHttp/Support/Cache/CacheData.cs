using UnityEngine;
using System;
using System.Text;

namespace UniHttp
{
	[Serializable()]
	internal sealed class CacheData
	{
		internal string domain;
		internal string path;
		internal string contentType;
		internal string eTag;
		internal DateTimeOffset? expireAt;
		internal DateTimeOffset? lastModified;
		internal DateTimeOffset createdAt;

		internal CacheData(HttpResponse response)
		{
			this.createdAt = DateTimeOffset.Now;
			Update(response);
		}

		internal void Update(HttpResponse response)
		{
			Uri uri = response.Request.Uri;

			this.domain = uri.Authority;
			this.path = uri.AbsolutePath;
			if(response.Headers.Exist(HeaderField.ContentType)) {
				this.contentType = response.Headers[HeaderField.ContentType][0];
			}
			if(response.Headers.Exist(HeaderField.ETag)) {
				this.eTag = response.Headers[HeaderField.ETag][0];
			}
			if(response.Headers.Exist(HeaderField.Expires)) {
				this.expireAt = DateTimeOffset.Parse(response.Headers[HeaderField.Expires][0]);
			}
			if(response.Headers.Exist(HeaderField.LastModified)) {
				this.lastModified = DateTimeOffset.Parse(response.Headers[HeaderField.LastModified][0]);
			}
			if(response.Headers.Exist(HeaderField.CacheControl) && response.Headers[HeaderField.CacheControl][0].Contains("max-age")) {
				foreach(string directive in response.Headers[HeaderField.CacheControl][0].Split(',')) {
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
