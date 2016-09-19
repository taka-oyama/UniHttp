using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace UniHttp
{
	public class ResponseHeaders
	{
		Dictionary<string, List<string>> fields;

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

		public bool Exist(string name)
		{
			return fields.ContainsKey(name.ToLower());
		}

		public bool NotExist(string name)
		{
			return !Exist(name);
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
				fieldLines.Add(Titleize(kvPair.Key) + ": " + string.Join(",", kvPair.Value.ToArray()));
			}
			return string.Join("\n", fieldLines.ToArray());
		}

		string Titleize(string str)
		{
			return string.Join("-", str.Split('-').Select(s => s.Substring(0, 1).ToUpper() + s.Substring(1)).ToArray());
		}
	}
}
