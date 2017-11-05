using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Net.Sockets;

namespace UniHttp
{
	public sealed class HttpManager : MonoBehaviour
	{
		HttpSettings settings;
		CookieJar cookieJar;
		CacheHandler cacheHandler;
		StreamPool streamPool;
		Messenger messenger;

		List<DispatchInfo> ongoingRequests;
		Queue<DispatchInfo> pendingRequests;
		Queue<Action> mainThreadQueue;

		object locker;
		float deltaTimer;

		public static HttpManager Initalize(HttpSettings httpSettings = null, bool dontDestroyOnLoad = true)
		{
			if(GameObject.Find("HttpManager")) {
				throw new Exception("HttpManager should not be Initialized more than once");
			}

			GameObject go = new GameObject("HttpManager");
			if(dontDestroyOnLoad) {
				GameObject.DontDestroyOnLoad(go);
			}

			return go.AddComponent<HttpManager>().Setup(httpSettings);
		}

		public HttpManager Setup(HttpSettings httpSettings = null)
		{
			this.settings = (httpSettings ?? new HttpSettings()).FillWithDefaults();

			string dataPath = settings.dataDirectory + "/UniHttp";
			Directory.CreateDirectory(dataPath);

			this.cookieJar = new CookieJar(settings.fileHandler, dataPath);
			this.cacheHandler = new CacheHandler(settings.fileHandler, dataPath);
			this.streamPool = new StreamPool(settings);
			this.messenger = new Messenger(settings, streamPool, cacheHandler, cookieJar);

			this.ongoingRequests = new List<DispatchInfo>();
			this.pendingRequests = new Queue<DispatchInfo>();
			this.mainThreadQueue = new Queue<Action>();
			this.locker = new object();
			this.deltaTimer = 0f;

			UserAgent.Build();

			return this;
		}

		public void Send(HttpRequest request, Action<HttpResponse> onResponse)
		{
			pendingRequests.Enqueue(new DispatchInfo(request, onResponse));
			TransmitIfPossible();
		}

		void TransmitIfPossible()
		{
			if(pendingRequests.Count > 0) {
				if(ongoingRequests.Count < settings.maxConcurrentRequests) {
					TransmitInWorkerThread();
				}
			}
		}

		void TransmitInWorkerThread()
		{
			DispatchInfo info = pendingRequests.Dequeue();
			ongoingRequests.Add(info);

			ThreadPool.QueueUserWorkItem(state => {
				try {
					HttpResponse response = messenger.Send(info.Request);
					ExecuteOnMainThread(() => {
						ongoingRequests.Remove(info);
						if(info.OnResponse != null) {
							info.OnResponse(response);
						}
						TransmitIfPossible();
					});
				} catch(Exception exception) {
					ExecuteOnMainThread(() => {
						throw exception;
					});
				}
			});
		}

		void ExecuteOnMainThread(Action callback)
		{
			lock(locker) {
				mainThreadQueue.Enqueue(callback);
			}
		}

		void Update()
		{
			while(mainThreadQueue.Count > 0) {
				mainThreadQueue.Dequeue().Invoke();
			}

			deltaTimer += Time.deltaTime;
			if (deltaTimer >= 1f) {
				UpdateEverySecond();
				deltaTimer = 0f;
			}
		}

		void UpdateEverySecond()
		{
			streamPool.CheckExpiredStreams();
		}

		void Save()
		{
			cookieJar.SaveToFile();
			cacheHandler.SaveToFile();
		}

		void OnApplicationPause(bool isPaused)
		{
			if(isPaused) {
				Save();
			}
		}

		void OnApplicationQuit()
		{
			streamPool.CloseAll();
			Save();
		}

		public void ClearCache()
		{
			Directory.Delete(settings.dataDirectory + "/UniHttp", true);
		}
	}
}
