using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniHttp
{
	internal class CookieDomain : IEnumerable
	{
		internal readonly string name;
		internal readonly List<Cookie> cookies; 

		internal CookieDomain(string name) {
			this.name = name;
			this.cookies = new List<Cookie>();
		}

		internal Cookie FindMatch(Cookie matcher) {
			foreach(Cookie cookie in cookies) {
				if(matcher.name == cookie.name) {
					return cookie;
				}
			}
			return null;
		}

		internal void AddOrReplace(Cookie cookie) {
			Cookie target = FindMatch(cookie);
			if(target != null) {
				cookies.Remove(target);
			}
			if(!cookie.IsExpired) {
				cookies.Add(cookie);
			}
		}

		internal List<Cookie> FindPersistedCookies()
		{
			return cookies.FindAll(c => !c.IsSession);
		}

		internal void RemoveExpiredCookies()
		{
			cookies.RemoveAll(c => c.IsExpired);
		}

		public IEnumerator GetEnumerator()
		{
			return cookies.GetEnumerator();
		}
	}
}
