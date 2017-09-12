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

		public static ILogger Logger;
		public static ISslVerifier SslVerifier;
		public static IFileHandler FileHandler;

		internal static Queue<Action> MainThreadQueue;
		internal static HttpStreamPool StreamPool;
		internal static CookieJar CookieJar;
		internal static CacheHandler CacheHandler;

		public static HttpManager Initalize(string dataDirectory = null, int maxPersistentConnections = 6, bool dontDestroyOnLoad = true)
		{
			if(GameObject.Find("HttpManager")) {
				throw new Exception("HttpManager should not be Initialized more than once");
			}
			GameObject go = new GameObject("HttpManager");
			if(dontDestroyOnLoad) {
				GameObject.DontDestroyOnLoad(go);
			}
			Directory.CreateDirectory(dataDirectory);
			return go.AddComponent<HttpManager>().Setup(dataDirectory, maxPersistentConnections);
		}

		HttpManager Setup(string baseDataDirectory, int maxPersistentConnections)
		{
			this.dataPath = (baseDataDirectory ?? Application.temporaryCachePath) + "/UniHttp";
			this.maxPersistentConnections = maxPersistentConnections;

			Logger = Logger ?? Debug.unityLogger;
			SslVerifier = SslVerifier ?? new DefaultSslVerifier();
			FileHandler = FileHandler ?? new DefaultFileHandler();

			MainThreadQueue = new Queue<Action>();
			StreamPool = new HttpStreamPool(maxPersistentConnections);

			CookieJar = new CookieJar(FileHandler, dataPath);
			CacheHandler = new CacheHandler(FileHandler, dataPath);

			return this;
		}

		void FixedUpdate()
		{
			while(MainThreadQueue.Count > 0) {
				MainThreadQueue.Dequeue().Invoke();
			}
		}

		void Save()
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
			StreamPool.CloseAll();
		}
	}
}
