using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace UniHttp
{
	public sealed class HttpManager : MonoBehaviour
	{
		public string dataPath;
		public int maxPersistentConnections;

		public static IContentSerializer RequestBodySerializer;
		public static ISslVerifier SslVerifier;
		public static ICacheStorage CacheStorage;

		internal static Queue<Action> MainThreadQueue;
		internal static HttpStreamPool TcpConnectionPool;
		internal static CookieJar CookieJar;
		internal static CacheHandler CacheHandler;

		public static void Initalize(string dataPath = null, int maxPersistentConnections = 6)
		{
			if(GameObject.Find("HttpManager")) {
				throw new Exception("HttpManager should not be Initialized more than once");
			}
			GameObject go = new GameObject("HttpManager");
			go.AddComponent<HttpManager>().Setup(dataPath, maxPersistentConnections);
			GameObject.DontDestroyOnLoad(go);
		}

		void Setup(string dataPath, int maxPersistentConnections)
		{
			this.dataPath = dataPath = dataPath ?? Application.temporaryCachePath + "/UniHttp/";
			this.maxPersistentConnections = maxPersistentConnections;
			Directory.CreateDirectory(dataPath);

			RequestBodySerializer = new JsonSerializer();
			SslVerifier = new DefaultSslVerifier();
			CacheStorage = new DefaultCacheStorage(new PersistentId(new FileInfo(dataPath + "id.key")).Fetch());

			MainThreadQueue = new Queue<Action>();
			TcpConnectionPool = new HttpStreamPool(maxPersistentConnections);
			CookieJar = new CookieJar(new FileInfo(dataPath + "Cookie.bin"));
			CacheHandler = new CacheHandler(new DirectoryInfo(dataPath + "Cache/"), CacheStorage);
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
