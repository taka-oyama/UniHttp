using UnityEngine;
using System.Text;
using System;

namespace UniHttp
{
	public sealed class HttpJsonData : IHttpData
	{
		string json;

		public HttpJsonData(object target)
		{
			this.json = HttpManager.RequestBodySerializer.Serialize(target);
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
			return Encoding.UTF8.GetBytes(ToString());
		}
	}
}
	