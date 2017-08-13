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
		List<DispatchInfo> ongoingRequests;
		Queue<DispatchInfo> pendingRequests;

		public HttpClient(HttpSetting? setting = null)
		{
			this.setting = setting.HasValue ? setting.Value : HttpSetting.Default;
			this.transport = new ConnectionHandler(this.setting);
			this.ongoingRequests = new List<DispatchInfo>();
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
					ongoingRequests.Add(info);
					SendInWorkerThread(info);
				}
			}
		}

		void SendInWorkerThread(DispatchInfo info)
		{
			ThreadPool.QueueUserWorkItem(state => {
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
						ongoingRequests.Remove(info);
						ExecuteIfPossible();
					});
				}
			});
		}

		void ExecuteOnMainThread(Action callback)
		{
			lock(locker) {
				HttpManager.MainThreadQueue.Enqueue(callback);
			}
		}
	}
}
