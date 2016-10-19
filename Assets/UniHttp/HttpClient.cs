using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

namespace UniHttp
{
	public sealed class HttpClient
	{
		public HttpSetting setting;

		RequestPreprocessor requestProcessor;
		ResponsePostprocessor responseProcessor;
		HttpStreamPool connectionPool;
		List<HttpRequest> ongoingRequests;
		Queue<HttpDispatchInfo> pendingRequests;

		public HttpClient(HttpSetting? setting = null)
		{
			this.setting = setting.HasValue ? setting.Value : HttpSetting.Default;

			var cookieJar = HttpManager.CookieJar;
			var cacheHandler = HttpManager.CacheHandler;

			this.requestProcessor = new RequestPreprocessor(this.setting, cookieJar, cacheHandler);
			this.responseProcessor = new ResponsePostprocessor(this.setting, cookieJar, cacheHandler);
			this.connectionPool = HttpManager.TcpConnectionPool;
			this.ongoingRequests = new List<HttpRequest>();
			this.pendingRequests = new Queue<HttpDispatchInfo>();
		}

		public void Send(HttpRequest request, Action<HttpResponse> callback)
		{
			requestProcessor.Execute(request);
			pendingRequests.Enqueue(new HttpDispatchInfo(request, callback));
			ExecuteIfPossible();
		}

		void ExecuteIfPossible()
		{
			if(pendingRequests.Count > 0) {
				if(ongoingRequests.Count < setting.maxConcurrentRequests) {
					var info = pendingRequests.Dequeue();
					ongoingRequests.Add(info.request);
					ThreadPool.QueueUserWorkItem(SendInThread, info);
				}
			}
		}

		void SendInThread(object obj)
		{
			var info = (HttpDispatchInfo)obj;

			try {
				var stream = connectionPool.CheckOut(info.request);
				var response = new HttpDispatcher(info).SendWith(stream);
				responseProcessor.Execute(response);
				connectionPool.CheckIn(response, stream);

				ExecuteOnMainThread(() => {
					if(info.callback != null) {
						info.callback(response);
					}
				});
			} catch(Exception exception) {
				ExecuteOnMainThread(() => {
					throw exception;
				});
			} finally {
				ExecuteOnMainThread(() => {
					ongoingRequests.Remove(info.request);
					ExecuteIfPossible();
				});
			}
		}

		void ExecuteOnMainThread(Action callback)
		{
			HttpManager.MainThreadQueue.Enqueue(callback);
		}
	}
}
