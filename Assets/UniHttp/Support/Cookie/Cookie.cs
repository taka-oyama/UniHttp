using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniHttp
{
	[Serializable]
	public sealed class Cookie
	{
		public string name;
		public string value;
		public string domain;
		public string path;
		public DateTime? expires;
		public bool secure;
		public bool httpOnly;

		internal string original;
		/// <summary>
		/// used for calculating total size of cookies per domain (4096 bytes)
		/// </summary>
		internal int size;

		public bool IsSession { get { return expires == null; } }
		public bool IsExpired { get { return expires.HasValue && expires < DateTime.Now; } }

		public override string ToString()
		{
			List<string> list = new List<string>();
			list.Add(name + "=" + value);
			if(!string.IsNullOrEmpty(domain)) {
				list.Add("Domain=" + domain);
			}
			list.Add("Path=" + path);
			if(secure) {
				list.Add("Secure");
			}
			if(httpOnly) {
				list.Add("HttpOnly");
			}
			return string.Join("; ", list.ToArray());
		}
	}
}
