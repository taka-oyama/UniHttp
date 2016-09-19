using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniHttp
{
	[Serializable]
	public class Cookie
	{
		public string Name;
		public string Value;

		public string Domain;
		public string Path;
		public DateTime? Expires;
		public bool Secure;
		public bool HttpOnly;

		internal bool ExactMatchOnly;

		public bool IsSession { get { return Expires == null; } }
		public bool IsExpired { get { return Expires.HasValue && Expires < DateTime.Now; } }

		public override string ToString()
		{
			List<string> list = new List<string>();
			list.Add(Name + "=" + Value);
			list.Add("Domain=" + Domain);
			list.Add("Path=" + Path);
			if(Secure) list.Add("Secure");
			if(HttpOnly) list.Add("HttpOnly");
			return string.Join("; ", list.ToArray());
		}
	}
}
