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
		public string encryptionPassword;

		public static IContentSerializer RequestBodySerializer;
		public static ISslVerifier SslVerifier;
		public static CacheStorage CacheStorage;

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

		void Setup(string baseDataPath, int maxPersistentConnections)
		{
			this.dataPath = baseDataPath ?? Application.temporaryCachePath + "/UniHttp/";
			this.maxPersistentConnections = maxPersistentConnections;
			this.encryptionPassword = Application.bundleIdentifier;
			Directory.CreateDirectory(dataPath);

			RequestBodySerializer = new JsonSerializer();
			SslVerifier = new DefaultSslVerifier();
			CacheStorage = new CacheStorage(new DirectoryInfo(dataPath + "Cache/"), encryptionPassword);

			MainThreadQueue = new Queue<Action>();
			TcpConnectionPool = new HttpStreamPool(maxPersistentConnections);
			CookieJar = new CookieJar(new SecureFileIO(dataPath + "Cookie.bin", encryptionPassword));
			CacheHandler = new CacheHandler(new SecureFileIO(dataPath + "CacheInfo.bin", encryptionPassword), CacheStorage);
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
			CacheHandler.SaveToFile();
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
