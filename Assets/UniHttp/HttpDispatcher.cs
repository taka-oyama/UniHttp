using UnityEngine;

namespace UniHttp
{
	public class HttpDispatcher
	{
		static GameObject go;

		public static IJsonSerializer JsonSerializer = new DefaultJsonSerializer();
		public static ISslVerifier SslVerifier = new NoSslVerifier();

		public static void Initalize()
		{
			if(go == null) {
				go = new GameObject("HttpMaintenance");
				go.AddComponent<HttpMaintenance>();
				GameObject.DontDestroyOnLoad(go);
			}
		}
	}
}
