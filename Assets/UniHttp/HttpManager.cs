using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;

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

		float deltaTimer;

		public static HttpManager Initalize(HttpSettings httpSettings = null, bool dontDestroyOnLoad = true)
		{
			string name = typeof(HttpManager).FullName;

			if(GameObject.Find(name)) {
				throw new Exception(name + " should not be Initialized more than once");
			}

			GameObject go = new GameObject(name);
			if(dontDestroyOnLoad) {
				DontDestroyOnLoad(go);
			}

			return go.AddComponent<HttpManager>().Setup(httpSettings);
		}

		public HttpManager Setup(HttpSettings httpSettings = null)
		{
			this.settings = (httpSettings ?? new HttpSettings()).FillWithDefaults();

			string dataPath = string.Concat(settings.dataDirectory, "/", GetType().Namespace);
			Directory.CreateDirectory(dataPath);

			this.streamPool = new StreamPool(settings);
			this.cookieJar = new CookieJar(settings.fileHandler, dataPath);
			this.cacheHandler = new CacheHandler(settings.fileHandler, dataPath);
			this.messenger = new Messenger(settings, streamPool, cacheHandler, cookieJar);

			this.ongoingRequests = new List<DispatchInfo>();
			this.pendingRequests = new Queue<DispatchInfo>();
			this.mainThreadQueue = new Queue<Action>();
			this.deltaTimer = 0f;

			UserAgent.Build();

			return this;
		}

		public void Send(HttpRequest request, Action<HttpResponse> onResponse)
		{
			pendingRequests.Enqueue(new DispatchInfo(request, onResponse));
			TransmitIfPossible();
		}

		public void ClearCache()
		{
			Directory.Delete(string.Concat(settings.dataDirectory, "/", GetType().Namespace), true);
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
				}
				catch(Exception exception) {
					ExecuteOnMainThread(() => {
						throw exception;
					});
				}
			});
		}

		void ExecuteOnMainThread(Action callback)
		{
			lock(mainThreadQueue) {
				mainThreadQueue.Enqueue(callback);
			}
		}

		void Update()
		{
			while(mainThreadQueue.Count > 0) {
				mainThreadQueue.Dequeue().Invoke();
			}

			// Update every second
			deltaTimer += Time.deltaTime;
			if(deltaTimer >= 1f) {
				streamPool.CheckExpiredStreams();
				deltaTimer = 0f;
			}
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

		void OnDestroy()
		{
			streamPool.CloseAll();
			Save();
		}
	}
}
