using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UniHttp
{
	public sealed class ResponseHeaders
	{
		readonly Dictionary<string, List<string>> fields;

		public ResponseHeaders()
		{
			this.fields = new Dictionary<string, List<string>>();
		}

		public List<string> this[string name]
		{
			get { return fields[name.ToLower()]; }
		}

		public int Length
		{
			get { return fields.Count; }
		}

		public bool Contains(string name)
		{
			return fields.ContainsKey(name.ToLower());
		}

		public bool Contains(string name, string value)
		{
			return Contains(name) && this[name].Any(each => each.Contains(value));
		}

		public bool NotContains(string name)
		{
			return !Contains(name);
		}

		internal void Append(string name, List<string> values)
		{
			for(int i = 0; i < values.Count; i++) {
				Append(name, values[0]);
			}
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
			foreach(KeyValuePair<string, List<string>> kvPair in fields) {
				foreach(string line in kvPair.Value) {
					fieldLines.Add(Titleize(kvPair.Key) + ": " + line);
				}
			}
			return string.Join(Constant.CRLF, fieldLines.ToArray());
		}

		static string Titleize(string str)
		{
			return string.Join("-", str.Split('-').Select(s => s.Substring(0, 1).ToUpper() + s.Substring(1)).ToArray());
		}
	}
}
