using UnityEngine;

namespace UniHttp
{
	public class DefaultJsonSerializer : IJsonSerializer
	{
		public string Serialize<T>(T target)
		{
			return JsonUtility.ToJson(target);
		}

		public T Deserialize<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}
	}
}
