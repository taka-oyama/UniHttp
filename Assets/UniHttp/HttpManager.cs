using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace UniHttp
{
	public sealed class HttpManager : MonoBehaviour
	{
		HttpContext context;
		CookieJar cookieJar;
		CacheHandler cacheHandler;
		StreamPool streamPool;
		Messenger messenger;
		float deltaTimer = 0f;

		List<DispatchInfo> processingRequests;
		Queue<DispatchInfo> pendingRequests;

		public static HttpManager Initalize(HttpContext httpContext = null, string name = null, bool dontDestroyOnLoad = true)
		{
			name = name ?? typeof(HttpManager).FullName;

			if(GameObject.Find(name)) {
				throw new Exception(name + " should not be Initialized more than once");
			}

			GameObject go = new GameObject(name);
			if(dontDestroyOnLoad) {
				DontDestroyOnLoad(go);
			}

			return go.AddComponent<HttpManager>().Setup(httpContext);
		}

		HttpManager Setup(HttpContext httpContext)
		{
			context = (httpContext ?? new HttpContext()).FillWithDefaults();

			string dataPath = string.Concat(context.dataDirectory, "/", GetType().Namespace);
			Directory.CreateDirectory(dataPath);
			UserAgent.Build();

			streamPool = new StreamPool(context.sslVerifier);
			cookieJar = new CookieJar(context.fileHandler, dataPath);
			cacheHandler = new CacheHandler(context.fileHandler, dataPath);
			messenger = new Messenger(context, streamPool, cacheHandler, cookieJar);
			processingRequests = new List<DispatchInfo>();
			pendingRequests = new Queue<DispatchInfo>();

			return this;
		}

		public async Task<HttpResponse> SendAsync(HttpRequest request)
		{
			DispatchInfo info = new DispatchInfo(request);
			pendingRequests.Enqueue(info);
			#pragma warning disable CS4014
			TransmitIfPossibleAsync();
			#pragma warning restore CS4014
			return await info.taskCompletion.Task;
		}

		async Task TransmitIfPossibleAsync()
		{
			if(pendingRequests.Count == 0) {
				return;
			}

			if(processingRequests.Count >= context.maxConcurrentRequests) {
				return;
			}

			DispatchInfo info = pendingRequests.Dequeue();
			if(!info.IsDisposed) {
				processingRequests.Add(info);
				HttpResponse response = await messenger.SendAsync(info.request, info.cancellationToken);
				processingRequests.Remove(info);
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

		public void ClearCache()
		{
			Directory.Delete(string.Concat(context.dataDirectory, "/", GetType().Namespace), true);
		}

		void Update()
		{
			// Update every second
			deltaTimer += Time.deltaTime;
			if(deltaTimer >= 1f) {
				streamPool.CheckExpiredStreams();
				deltaTimer = 0f;
			}
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
