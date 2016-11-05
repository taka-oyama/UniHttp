using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace UniHttp
{
	public sealed class HttpManager : MonoBehaviour
	{
		static GameObject go;

		public static int maxPersistentConnections = 6;
		public static string DataPath = Application.temporaryCachePath + "/UniHttp/";
		public static IJsonSerializer JsonSerializer = new DefaultJsonSerializer();
		public static ISslVerifier SslVerifier = new NoSslVerifier();

		internal static Queue<Action> MainThreadQueue;
		internal static HttpStreamPool TcpConnectionPool;
		internal static CookieJar CookieJar;
		internal static CacheHandler CacheHandler;

		public static void Initalize()
		{
			Directory.CreateDirectory(DataPath);
			MainThreadQueue = new Queue<Action>();
			TcpConnectionPool = new HttpStreamPool(maxPersistentConnections);
			CookieJar = new CookieJar(new FileInfo(DataPath + "Cookie.bin"));
			CacheHandler = new CacheHandler(new DirectoryInfo(DataPath + "Cache/"));

			if(go == null) {
				go = new GameObject("HttpManager");
				go.AddComponent<HttpManager>();
				GameObject.DontDestroyOnLoad(go);
			}
		}

		void FixedUpdate()
		{
			while(MainThreadQueue.Count > 0) {
				MainThreadQueue.Dequeue().Invoke();
			}
		}

		internal static void Save()
		{
			CookieJar.SaveToFile();
		}

		void OnApplicationPause(bool isPaused)
		{
			if(isPaused) {
				Save();
			}
		}

		void OnApplicationQuit()
		{
			Save();
		}
	}
}
