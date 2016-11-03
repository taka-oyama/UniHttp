using UnityEngine;
using System.Text;

namespace UniHttp
{
	public sealed class HttpJsonData : IHttpData
	{
		string json;

		public HttpJsonData(object target)
		{
			this.json = HttpManager.JsonSerializer.Serialize(target);
		}

		public HttpJsonData(string json)
		{
			this.json = json;
		}

		public string GetContentType()
		{
			return "application/json";
		}

		public override string ToString()
		{
			return json;
		}

		public byte[] ToBytes()
		{
			return Encoding.UTF8.GetBytes(json);
		}
	}
}
	