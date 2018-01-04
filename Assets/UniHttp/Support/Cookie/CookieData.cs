using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniHttp
{
	[Serializable]
	public sealed class CookieData
	{
		public string name;
		public string value;
		public string domain;
		public string path;
		public DateTime? expires;
		public bool secure;
		public bool httpOnly;

		// not part of spec but exists for determining when to cleanup.
		public DateTime CreatedAt;

		// Set true when domain is not defined. See link below for details.
		// https://en.wikipedia.org/wiki/HTTP_cookie#Domain_and_Path
		public bool ExactMatchOnly;

		public bool IsSession { get { return expires == null; } }
		public bool IsExpired { get { return expires.HasValue && expires < DateTime.Now; } }

		public override string ToString()
		{
			List<string> list = new List<string>();
			list.Add(name + "=" + value);
			list.Add("Domain=" + domain);
			list.Add("Path=" + path);
			if(secure) list.Add("Secure");
			if(httpOnly) list.Add("HttpOnly");
			return string.Join("; ", list.ToArray());
		}
	}
}
