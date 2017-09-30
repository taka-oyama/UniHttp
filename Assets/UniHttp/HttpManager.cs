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
		HttpStreamPool streamPool;
		CookieJar cookieJar;
		CacheHandler cacheHandler;
		ResponseBuilder responseBuilder;
		RequestPreprocessor requestPreprocessor;
		ResponsePostprocessor responsePostprocessor;

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

			this.streamPool = new HttpStreamPool(settings.keepAliveTimeout, settings.sslVerifier);
			this.cookieJar = new CookieJar(settings.fileHandler, dataPath);
			this.cacheHandler = new CacheHandler(settings.fileHandler, dataPath);
			this.responseBuilder = new ResponseBuilder(cacheHandler);
			this.requestPreprocessor = new RequestPreprocessor(settings, cookieJar, cacheHandler);
			this.responsePostprocessor = new ResponsePostprocessor(settings, cookieJar, cacheHandler);

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
					HttpResponse response = Transmit(info);
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

		HttpResponse Transmit(DispatchInfo info)
		{
			HttpRequest request = info.Request;
			HttpResponse response = null;

			try {
				while(true) {
					requestPreprocessor.Execute(request);

					// Log request
					settings.logger.Log(string.Concat(request.Uri, Constant.CRLF, request));

					// Send request though TCP stream
					byte[] data = request.ToBytes();
					HttpStream stream = streamPool.CheckOut(request);
					stream.Write(data, 0, data.Length);
					stream.Flush();

					// Build the response from stream
					response = responseBuilder.Build(request, stream);
					streamPool.CheckIn(response, stream);
					responsePostprocessor.Execute(response);

					// Log response
					settings.logger.Log(string.Concat(response.Request.Uri, Constant.CRLF, response));

					// Handle redirects
					if(IsRedirect(response)) {
						request = MakeRedirectRequest(response);
					} else {
						break;
					}
				}
			}
			catch(SocketException exception) {
				response = responseBuilder.Build(request, exception);
				settings.logger.Log(string.Concat(response.Request.Uri, Constant.CRLF, response));
			}

			return response;
		}

		bool IsRedirect(HttpResponse response)
		{
			if(settings.followRedirects) {
				for(int i = 0; i < Constant.Redirects.Length; i++) {
					if(response.StatusCode == Constant.Redirects[i]) {
						return true;
					}
				}
			}
			return false;
		}

		HttpRequest MakeRedirectRequest(HttpResponse response)
		{
			Uri uri = new Uri(response.Headers["Location"][0]);
			HttpRequest request = response.Request;
			HttpMethod method = request.Method;
			if(response.StatusCode == StatusCode.SeeOther) {
				if(method == HttpMethod.POST || method == HttpMethod.PUT || method == HttpMethod.PATCH) {
					method = HttpMethod.GET;
				}
			}

			request.Headers.Remove("Host");

			return new HttpRequest(method, uri, request.Headers, request.Data);
		}

		void FixedUpdate()
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
			Save();
			streamPool.CloseAll();
		}
	}
}
