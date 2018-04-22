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

		public static HttpManager Initalize(HttpContext httpContext = null, bool dontDestroyOnLoad = true)
		{
			string name = typeof(HttpManager).FullName;

			if(GameObject.Find(name)) {
				throw new Exception(name + " should not be Initialized more than once");
			}

			GameObject go = new GameObject(name);
			if(dontDestroyOnLoad) {
				DontDestroyOnLoad(go);
			}

			return go.AddComponent<HttpManager>().Setup(httpContext);
		}

		HttpManager Setup(HttpContext httpContext = null)
		{
			this.context = (httpContext ?? new HttpContext()).FillWithDefaults();

			string dataPath = string.Concat(context.dataDirectory, "/", GetType().Namespace);
			Directory.CreateDirectory(dataPath);

			this.streamPool = new StreamPool(context.sslVerifier);
			this.cookieJar = new CookieJar(context.fileHandler, dataPath);
			this.cacheHandler = new CacheHandler(context.fileHandler, dataPath);
			this.messenger = new Messenger(context, streamPool, cacheHandler, cookieJar);

			this.processingRequests = new List<DispatchInfo>();
			this.pendingRequests = new Queue<DispatchInfo>();

			UserAgent.Build();

			return this;
		}

		public async Task<HttpResponse> DeleteAsync(Uri uri, IHttpData data = null)
		{
			return await SendAsync(new HttpRequest(HttpMethod.DELETE, uri, data));
		}

		public async Task<HttpResponse> GetAsync(Uri uri, HttpQuery query = null)
		{
			return await SendAsync(new HttpRequest(HttpMethod.GET, uri, query));
		}

		public async Task<HttpResponse> PatchAsync(Uri uri, IHttpData data = null)
		{
			return await SendAsync(new HttpRequest(HttpMethod.PATCH, uri, data));
		}

		public async Task<HttpResponse> PostAsync(Uri uri, IHttpData data = null)
		{
			return await SendAsync(new HttpRequest(HttpMethod.POST, uri, data));
		}

		public async Task<HttpResponse> PutAsync(Uri uri, IHttpData data = null)
		{
			return await SendAsync(new HttpRequest(HttpMethod.PUT, uri, data));
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

		public void ClearCache()
		{
			Directory.Delete(string.Concat(context.dataDirectory, "/", GetType().Namespace), true);
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

		void Update()
		{
			// Update every second
			deltaTimer += Time.deltaTime;
			if(deltaTimer >= 1f) {
				streamPool.CheckExpiredStreams();
				deltaTimer = 0f;
			}
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
