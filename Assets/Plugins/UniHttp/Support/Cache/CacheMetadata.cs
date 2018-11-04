using UnityEngine;
using System;
using System.Text;

namespace UniHttp
{
	internal sealed class CacheMetadata
	{
		internal const int version = 1;

		internal string domain;
		internal string path;
		internal string contentType;
		internal string eTag;
		internal DateTime? expireAt;
		internal DateTime? lastModified;

		internal bool IsFresh => expireAt > DateTime.Now;
		internal bool IsStale => !IsFresh;

		internal CacheMetadata()
		{
		}

		internal CacheMetadata(HttpResponse response)
		{
			Uri uri = response.Request.Uri;

			this.domain = uri.Authority;
			this.path = uri.AbsolutePath;
			if(response.Headers.Contains(HeaderField.ContentType)) {
				this.contentType = response.Headers[HeaderField.ContentType][0];
			}
			if(response.Headers.Contains(HeaderField.ETag)) {
				this.eTag = response.Headers[HeaderField.ETag][0];
			}
			if(response.Headers.Contains(HeaderField.Expires)) {
				this.expireAt = DateTime.Parse(response.Headers[HeaderField.Expires][0]);
			}
			if(response.Headers.Contains(HeaderField.CacheControl) && response.Headers[HeaderField.CacheControl][0].Contains(HeaderValue.MaxAge)) {
				foreach(string directive in response.Headers[HeaderField.CacheControl][0].Split(',')) {
					if(directive.Contains(HeaderValue.MaxAge)) {
						this.expireAt = DateTime.Now.AddSeconds(int.Parse(directive.Split('=')[1]));
					}
				}
			}
			if(response.Headers.Contains(HeaderField.LastModified)) {
				this.lastModified = DateTime.Parse(response.Headers[HeaderField.LastModified][0]);
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
