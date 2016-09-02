﻿using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	public class HttpResponseHeaders
	{
		Dictionary<string, List<string>> fields;

		public HttpResponseHeaders()
		{
			this.fields = new Dictionary<string, List<string>>();
		}

		public List<string> this[string name]
		{
			get { return fields[name.ToLower()]; }
		}

		public bool Exist(string name)
		{
			return fields.ContainsKey(name.ToLower());
		}

		internal void Append(string name, string value)
		{
			name = name.ToLower();
			if(fields.ContainsKey(name)) {
				fields[name].Add(value);
			} else {
				fields.Add(name, new List<string> { value });
			}
		}

		public override string ToString()
		{
			List<string> fieldLines = new List<string>();
			foreach(var kvPair in fields) {
				fieldLines.Add(kvPair.Key + ": " + string.Join(",", kvPair.Value.ToArray()));
			}
			return string.Join(HttpRequest.CRLF, fieldLines.ToArray());
		}
	}
}
