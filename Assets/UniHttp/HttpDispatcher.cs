using UnityEngine;
using System.IO;

namespace UniHttp
{
	public class HttpDispatcher
	{
		static GameObject go;

		public static string DataPath = Application.temporaryCachePath + "/UniHttp/";
		public static IJsonSerializer JsonSerializer = new DefaultJsonSerializer();
		public static ISslVerifier SslVerifier = new NoSslVerifier();

		internal static CookieJar CookieJar;
		internal static CacheHandler CacheHandler;

		public static void Initalize()
		{
			Directory.CreateDirectory(DataPath);

			CookieJar = new CookieJar(new FileInfo(DataPath + "Cookie.bin"));
			CacheHandler = new CacheHandler(new DirectoryInfo(DataPath + "Cache/"));

			if(go == null) {
				go = new GameObject("HttpManager");
				go.AddComponent<HttpManager>();
				GameObject.DontDestroyOnLoad(go);
			}
		}

		internal static void Save()
		{
			CookieJar.SaveToFile();
		}
	}
}
