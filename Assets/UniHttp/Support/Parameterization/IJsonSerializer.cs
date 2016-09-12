using UnityEngine;

namespace UniHttp
{
	public interface IJsonSerializer
	{
		string Serialize<T>(T target);
		T Deserialize<T>(string json);
	}
}
