using UnityEngine;
using System;

namespace UniHttp
{
	internal class DispatchInfo : IDisposable
	{
		readonly internal HttpRequest request;
		readonly internal CancellationToken cancellationToken;
		readonly Action<HttpResponse> onResponse;

		public bool IsDisposed { get; private set; }

		internal DispatchInfo(HttpRequest request, Action<HttpResponse> onResponse)
		{
			this.request = request;
			this.onResponse = onResponse;
			this.cancellationToken = new CancellationToken(this);
			this.IsDisposed = false;
		}

		internal void InvokeCallback(HttpResponse response)
		{
			if(!IsDisposed && onResponse != null) {
				onResponse.Invoke(response);
			}
		}

		public void Dispose()
		{
			if(!IsDisposed) {
				IsDisposed = true;
			}
		}
	}
}
