using UnityEngine;

namespace UniHttp
{
	public class JsonSerializer : IContentSerializer
	{
		public string Serialize<T>(T target)
		{
			return JsonUtility.ToJson(target);
		}
	}
}
