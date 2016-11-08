using UnityEngine;

namespace UniHttp
{
	public class JsonDeserializer : IContentDeserializer
	{
		public T Deserialize<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}
	}
}
