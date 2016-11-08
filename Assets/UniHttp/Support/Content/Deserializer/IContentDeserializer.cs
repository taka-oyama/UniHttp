using UnityEngine;

namespace UniHttp
{
	public interface IContentDeserializer
	{
		T Deserialize<T>(string json);
	}
}
