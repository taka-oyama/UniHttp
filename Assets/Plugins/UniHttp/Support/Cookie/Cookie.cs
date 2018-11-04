using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniHttp
{
	public sealed class Cookie
	{
		public string Name;
		public string Value;
		public string Domain;
		public string Path;
		public DateTime? Expires;
		public bool Secure;
		public bool HttpOnly;

		/// <summary>
		/// used for calculating total size of cookies per domain (4096 bytes)
		/// </summary>
		internal int Size;

		public bool IsSession => Expires == null;
		public bool IsExpired => Expires.HasValue && Expires < DateTime.Now;

		public override string ToString()
		{
			List<string> list = new List<string>();
			list.Add(Name + Constant.Equal + Value);
			if(!string.IsNullOrEmpty(Domain)) {
				list.Add("Domain=" + Domain);
			}
			list.Add("Path=" + Path);
			if(Secure) {
				list.Add("Secure");
			}
			if(HttpOnly) {
				list.Add("HttpOnly");
			}
			return string.Join("; ", list.ToArray());
		}
	}
}
