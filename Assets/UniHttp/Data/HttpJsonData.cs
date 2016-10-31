using UnityEngine;
using System.Text;

namespace UniHttp
{
	public sealed class HttpJsonData : IHttpData
	{
		object target;

		public HttpJsonData(object target)
		{
			this.target = target;
		}

		public string GetContentType()
		{
			return "application/json";
		}

		public override string ToString()
		{
			return HttpManager.JsonSerializer.Serialize(target);
		}

		public byte[] ToBytes()
		{
			return Encoding.UTF8.GetBytes(ToString());
		}
	}
}
	