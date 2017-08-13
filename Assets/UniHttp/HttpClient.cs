using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

namespace UniHttp
{
	public sealed class HttpClient
	{
		public HttpSetting setting;

		object locker = new object();
		ConnectionHandler transport;
		List<HttpRequest> ongoingRequests;
		Queue<DispatchInfo> pendingRequests;

		public HttpClient(HttpSetting? setting = null)
		{
			this.setting = setting.HasValue ? setting.Value : HttpSetting.Default;
			this.transport = new ConnectionHandler(this.setting);
			this.ongoingRequests = new List<HttpRequest>();
			this.pendingRequests = new Queue<DispatchInfo>();
		}

		public void Send(HttpRequest request, Action<HttpResponse> callback)
		{
			pendingRequests.Enqueue(new DispatchInfo(request, callback));
			ExecuteIfPossible();
		}

		void ExecuteIfPossible()
		{
			if(pendingRequests.Count > 0) {
				if(ongoingRequests.Count < setting.maxConcurrentRequests) {
					DispatchInfo info = pendingRequests.Dequeue();
					ongoingRequests.Add(info.Request);
					SendInThread(info);
				}
			}
		}

		void SendInThread(DispatchInfo info)
		{
			WrapInThread(() => {
				try {
					var response = transport.Send(info.Request);
					ExecuteOnMainThread(() => {
						if(info.Callback != null) {
							info.Callback(response);
						}
					});
				} catch(Exception exception) {
					ExecuteOnMainThread(() => {
						throw exception;
					});
				} finally {
					ExecuteOnMainThread(() => {
						ongoingRequests.Remove(info.Request);
						ExecuteIfPossible();
					});
				}
			});
		}

		void WrapInThread(Action action)
		{
			ThreadPool.QueueUserWorkItem(_ => action());
		}

		void ExecuteOnMainThread(Action callback)
		{
			lock(locker) {
				HttpManager.MainThreadQueue.Enqueue(callback);
			}
		}
	}
}
