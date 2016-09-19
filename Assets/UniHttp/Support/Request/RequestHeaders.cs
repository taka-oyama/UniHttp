﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UniHttp
{
	public class RequestHeaders
	{
		Dictionary<string, string> fields;

		public RequestHeaders()
		{
			this.fields = new Dictionary<string, string>();
		}

		public int Length
		{
			get { return fields.Count; }
		}

		public string this[string name]
		{
			get { return fields[name.ToLower()]; }
		}

		public bool Exist(string name)
		{
			name = name.ToLower();
			return fields.ContainsKey(name);
		}

		public bool NotExist(string name)
		{
			return !Exist(name);
		}

		public void Add(string name, string value)
		{
			name = name.ToLower();
			if(fields.ContainsKey(name)) throw new KeyNotFoundException("Key with name '" + name + "' already exists.");
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

		public void Remove(string name)
		{
			fields.Remove(name.ToLower());
		}

		public override string ToString()
		{
			List<string> fieldLines = new List<string>();
			foreach(var kvPair in fields) {
				fieldLines.Add(Titleize(kvPair.Key) + ": " + kvPair.Value);
			}
			return string.Join("\n", fieldLines.ToArray());
		}

		string Titleize(string str)
		{
			return string.Join("-", str.Split('-').Select(s => s.Substring(0, 1).ToUpper() + s.Substring(1)).ToArray());
		}
	}
}
