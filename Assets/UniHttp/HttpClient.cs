using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

namespace UniHttp
{
	public sealed class HttpClient
	{
		public HttpSetting setting;

		HttpMessanger dispatcher;
		List<HttpRequest> ongoingRequests;
		Queue<HttpDispatchInfo> pendingRequests;

		public HttpClient(HttpSetting? setting = null)
		{
			this.setting = setting.HasValue ? setting.Value : HttpSetting.Default;
			this.dispatcher = new HttpMessanger(this.setting);
			this.ongoingRequests = new List<HttpRequest>();
			this.pendingRequests = new Queue<HttpDispatchInfo>();
		}

		public void Send(HttpRequest request, Action<HttpResponse> callback)
		{
			pendingRequests.Enqueue(new HttpDispatchInfo(request, callback));
			ExecuteIfPossible();
		}

		void ExecuteIfPossible()
		{
			if(pendingRequests.Count > 0) {
				if(ongoingRequests.Count < setting.maxConcurrentRequests) {
					var info = pendingRequests.Dequeue();
					ongoingRequests.Add(info.request);
					ThreadPool.QueueUserWorkItem(_ => SendInThread(info));
				}
			}
		}

		void SendInThread(HttpDispatchInfo info)
		{
			try {
				var response = dispatcher.Send(info.request);

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
