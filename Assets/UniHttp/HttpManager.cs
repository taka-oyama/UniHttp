using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

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

			UserAgent.Build();

			return this;
		}

		public async Task<HttpResponse> SendAsync(HttpRequest request, Progress progress = null)
		{
			DispatchInfo info = new DispatchInfo(request, progress);
			pendingRequests.Enqueue(info);
			#pragma warning disable CS4014
			TransmitIfPossibleAsync();
			#pragma warning restore CS4014
			return await info.taskCompletion.Task;
		}

		public void ClearCache()
		{
			Directory.Delete(string.Concat(settings.dataDirectory, "/", GetType().Namespace), true);
		}

		async Task TransmitIfPossibleAsync()
		{
			if(pendingRequests.Count == 0) {
				return;
			}

			if(ongoingRequests.Count >= settings.maxConcurrentRequests) {
				return;
			}

			DispatchInfo info = pendingRequests.Dequeue();
			if(!info.IsDisposed) {
				ongoingRequests.Add(info);
				HttpResponse response = await messenger.SendAsync(info.request, info.downloadProgress, info.cancellationToken);
				ongoingRequests.Remove(info);
				info.SetResult(response);
			}

			#pragma warning disable CS4014
			TransmitIfPossibleAsync();
			#pragma warning restore CS4014
		}

		void Save()
		{
			cookieJar.WriteToFile();
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
