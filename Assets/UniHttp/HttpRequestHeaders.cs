﻿using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	public class HttpRequestHeaders
	{
		Dictionary<string, string> fields;

		public HttpRequestHeaders()
		{
			this.fields = new Dictionary<string, string>();
		}

		public bool Exist(string name)
		{
			name = name.ToLower();
			return fields.ContainsKey(name);
		}

		public void Add(string name, string value)
		{
			name = name.ToLower();
			if(fields.ContainsKey(name)) throw new KeyNotFoundException("Key with name '" + name + "' does not exist.");
			fields.Add(name, value);
		}

		public void AddOrReplace(string name, string value)
		{
			name = name.ToLower();
			if(fields.ContainsKey(name)) {
				fields[name] = value;
			} else {
				Add(name, value);
			}
		}

		public void Append(string name, string value)
		{
			name = name.ToLower();
			fields[name] = string.Join(",", new string[] { fields[name], value });
		}

		public void Remove(string name)
		{
			fields.Remove(name.ToLower());
		}

		public override string ToString()
		{
			List<string> fieldLines = new List<string>();
			foreach(var kvPair in fields) {
				fieldLines.Add(kvPair.Key + ": " + kvPair.Value);
			}
			return string.Join(HttpRequest.CRLF, fieldLines.ToArray());
		}
	}
}
