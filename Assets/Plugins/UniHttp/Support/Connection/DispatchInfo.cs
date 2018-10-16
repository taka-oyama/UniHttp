using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniHttp
{
	internal class DispatchInfo : IDisposable
	{
		internal HttpRequest Request { get; private set; }
		internal TaskCompletionSource<HttpResponse> TaskCompletion { get; private set; }
		readonly CancellationTokenSource cancellationTokenSource;
		internal CancellationToken CancellationToken { get; private set; }
		public bool IsDisposed { get; private set; }

		internal DispatchInfo(HttpRequest request)
		{
			this.Request = request;
			this.TaskCompletion = new TaskCompletionSource<HttpResponse>();
			this.cancellationTokenSource = new CancellationTokenSource();
			this.CancellationToken = cancellationTokenSource.Token;
			this.IsDisposed = false;
		}

		internal void SetResult(HttpResponse response)
		{
			TaskCompletion.SetResult(response);
		}

		public void Dispose()
		{
			TaskCompletion.SetCanceled();
			cancellationTokenSource.Cancel();
            if(!IsDisposed) {
				IsDisposed = true;
			}
		}
	}
}
