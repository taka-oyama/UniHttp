using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UniHttp
{
	public sealed class HttpQueryString
	{
		string prefix;
		string separator;
		string delimiter;
		Dictionary<string, List<string>> query;

		public HttpQueryString(string separator = "&", string delimiter = "=", string prefix = "")
		{
			this.prefix = prefix;
			this.separator = separator;
			this.delimiter = delimiter;
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
			List<string> kv = new List<string>();
			foreach(string name in query.Keys) {
				query[name].ForEach(value => kv.Add(string.Concat(prefix, name, delimiter, value)));
			}
			return string.Join(separator, kv.ToArray());
		}
	}
}
