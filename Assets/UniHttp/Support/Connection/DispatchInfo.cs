using UnityEngine;
using System;

namespace UniHttp
{
	public class DispatchInfo : IDisposable
	{
		readonly internal HttpRequest request;
		readonly internal CancellationToken cancellationToken;
        Action<HttpResponse> onResponse;

		public bool IsDisposed { get; private set; }

		public Progress DownloadProgress { get; private set; }

		internal DispatchInfo(HttpRequest request, Action<HttpResponse> onResponse)
		{
			this.request = request;
			this.onResponse = onResponse;
			this.cancellationToken = new CancellationToken(this);
			this.DownloadProgress = new Progress();
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
            this.onResponse = null;

            if(!IsDisposed) {
				IsDisposed = true;
			}
		}
	}
}
