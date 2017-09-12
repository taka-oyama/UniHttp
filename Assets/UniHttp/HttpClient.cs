using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;

namespace UniHttp
{
	public sealed class HttpClient
	{
		HttpSetting setting;
		ILogger logger;
		HttpStreamPool streamPool;
		ResponseBuilder responseBuilder;
		RequestPreprocessor requestPreprocessor;
		ResponsePostprocessor responsePostprocessor;

		List<DispatchInfo> ongoingRequests;
		Queue<DispatchInfo> pendingRequests;
		object locker = new object();

		public HttpClient(HttpSetting? httpSetting = null)
		{
			var cookieJar = HttpManager.CookieJar;
			var cacheHandler = HttpManager.CacheHandler;

			this.setting = httpSetting.HasValue ? httpSetting.Value : HttpSetting.Default;
			this.logger = HttpManager.Logger;
			this.streamPool = HttpManager.StreamPool;
			this.responseBuilder = new ResponseBuilder();
			this.requestPreprocessor = new RequestPreprocessor(setting, cookieJar, cacheHandler);
			this.responsePostprocessor = new ResponsePostprocessor(setting, cookieJar, cacheHandler);

			this.ongoingRequests = new List<DispatchInfo>();
			this.pendingRequests = new Queue<DispatchInfo>();
		}

		public void Send(HttpRequest request, Action<HttpResponse> callback)
		{
			pendingRequests.Enqueue(new DispatchInfo(request, callback));
			TransmitIfPossible();
		}

		void TransmitIfPossible()
		{
			if(pendingRequests.Count > 0) {
				if(ongoingRequests.Count < setting.maxConcurrentRequests) {
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
						if(info.Callback != null) {
							info.Callback(response);
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
				HttpManager.MainThreadQueue.Enqueue(callback);
			}
		}

		HttpResponse Transmit(DispatchInfo info)
		{
			HttpRequest request = info.Request;
			HttpResponse response = null;

			try {
				requestPreprocessor.Execute(request);

				while(true) {
					// Log request
					logger.Log(string.Concat(request.Uri, Constant.CRLF, request));

					// Send request though TCP stream
					HttpStream stream = streamPool.CheckOut(request);
					byte[] data = request.ToBytes();
					stream.Write(data, 0, data.Length);
					stream.Flush();

					// Build the response from stream
					response = responseBuilder.Build(request, stream);
					streamPool.CheckIn(response, stream);
					responsePostprocessor.Execute(response);

					// Log response
					logger.Log(string.Concat(response.Request.Uri, Constant.CRLF, response));

					// Handle redirects
					if(setting.followRedirects && IsRedirect(response)) {
						request = MakeRedirectRequest(response);
					} else {
						break;
					}
				}
			}
			catch(SocketException exception) {
				response = responseBuilder.Build(request, exception);
				logger.Log(string.Concat(response.Request.Uri, Constant.CRLF, response));
			}

			return response;
		}

		bool IsRedirect(HttpResponse response)
		{
			for(int i = 0; i < Constant.REDIRECTS.Length; i++) {
				if(response.StatusCode == Constant.REDIRECTS[i]) {
					return true;
				}
			}
			return false;
		}

		HttpRequest MakeRedirectRequest(HttpResponse response)
		{
			Uri uri = new Uri(response.Headers["Location"][0]);
			HttpRequest request = response.Request;
			HttpMethod method = request.Method;
			if(response.StatusCode == 303) {
				if(method == HttpMethod.POST || method == HttpMethod.PUT || method == HttpMethod.PATCH) {
				   method = HttpMethod.GET;
				}
			}

			request.Headers.Remove("Host");

			return new HttpRequest(method, uri, request.Headers, request.Data);
		}
	}
}
