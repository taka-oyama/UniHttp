using UnityEngine;

namespace UniHttp
{
	interface IHttpData
	{
		string GetContentType();
		byte[] ToBytes();
	}
}
