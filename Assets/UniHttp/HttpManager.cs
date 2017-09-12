using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace UniHttp
{
	public sealed class HttpManager : MonoBehaviour
	{
		public static ILogger Logger;
		public static ISslVerifier SslVerifier;

		internal static Queue<Action> MainThreadQueue;
		internal static HttpStreamPool StreamPool;
		internal static CookieJar CookieJar;
		internal static CacheHandler CacheHandler;

		public static HttpManager Initalize(HttpSettings settings = null, bool dontDestroyOnLoad = true)
		{
			if(GameObject.Find("HttpManager")) {
				throw new Exception("HttpManager should not be Initialized more than once");
			}

			GameObject go = new GameObject("HttpManager");
			if(dontDestroyOnLoad) {
				GameObject.DontDestroyOnLoad(go);
			}

			return go.AddComponent<HttpManager>().Setup(settings ?? new HttpSettings());
		}

		HttpManager Setup(HttpSettings settings)
		{
			settings.FillWithDefaults();

			Logger = settings.logger;
			SslVerifier = settings.sslVerifier;

			string dataPath = settings.dataDirectory + "/UniHttp";
			Directory.CreateDirectory(settings.dataDirectory);

			MainThreadQueue = new Queue<Action>();
			StreamPool = new HttpStreamPool(settings.maxPersistentConnections.Value);

			CookieJar = new CookieJar(settings.fileHandler, dataPath);
			CacheHandler = new CacheHandler(settings.fileHandler, dataPath);

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
