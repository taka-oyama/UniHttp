using UnityEngine;
using System.Text;

namespace UniHttp
{
	public class HttpFormUrlencoded : HttpQuery, IHttpData
	{
		public string GetContentType()
		{
			return "application/x-www-form-urlencoded";
		}

		public byte[] ToBytes()
		{
			return Encoding.ASCII.GetBytes(ToString());
		}
	}
}
