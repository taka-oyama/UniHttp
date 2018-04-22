using UnityEngine;

namespace UniHttp
{
	public interface IHttpData
	{
		string GetContentType();
		byte[] ToBytes();
	}
}
