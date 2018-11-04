using UnityEngine;
using System.Text;

namespace UniHttp
{
	public class HttpFormUrlencoded : HttpQuery, IHttpData
	{
		public string GetContentType()
		{
			return ContentType.FormUrlEncoded;
		}

		public byte[] ToBytes()
		{
			return Encoding.ASCII.GetBytes(ToString());
		}
	}
}
