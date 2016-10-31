using UnityEngine;
using System.Text;

namespace UniHttp
{	
	public class RequestDataBuilder
	{

		HttpRequest request;

		public RequestDataBuilder(HttpRequest request)
		{
			this.request = request;
		}

		public byte[] Build()
		{
			return Encoding.UTF8.GetBytes(request.ToString());
		}
	}
}
