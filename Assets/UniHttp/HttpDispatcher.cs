using UnityEngine;

namespace UniHttp
{
	public class HttpDispatcher
	{
		static GameObject go;

		public static string CachePath = Application.temporaryCachePath;
		public static IJsonSerializer JsonSerializer = new DefaultJsonSerializer();
		public static ISslVerifier SslVerifier = new NoSslVerifier();
		internal static CookieJar CookieJar;

		public static void Initalize()
		{
			CookieJar = new CookieJar(CachePath + "/Cookie.bin");

			if(go == null) {
				go = new GameObject("HttpMaintenance");
				go.AddComponent<HttpContext>();
				GameObject.DontDestroyOnLoad(go);
			}
		}

		internal static void Save()
		{
			CookieJar.SaveToFile();
		}
	}
}
