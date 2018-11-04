﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace UniHttp
{
	public class HttpQuery
	{
		Dictionary<string, List<string>> query;

		public HttpQuery()
		{
			this.query = new Dictionary<string, List<string>>();
		}

		public void Add(string name, object value)
		{
			if(!query.ContainsKey(name)) {
				query.Add(name, new List<string>());
			}
			query[name].Add(value.ToString());
		}

		public override string ToString()
		{
			List<string> kv = new List<string>(query.Count);
			foreach(string name in query.Keys) {
				foreach(string value in query[name]) {
					kv.Add(string.Concat(Uri.EscapeUriString(name), Constant.Equal, Uri.EscapeDataString(value)));
				}
			}
			return string.Join(Constant.Ampersand, kv.ToArray());
		}
	}
}
