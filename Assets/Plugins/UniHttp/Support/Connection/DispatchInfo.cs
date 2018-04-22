﻿using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniHttp
{
	internal class DispatchInfo : IDisposable
	{
		readonly internal HttpRequest request;
		readonly CancellationTokenSource cancellationTokenSource;
		readonly internal CancellationToken cancellationToken;
		readonly internal TaskCompletionSource<HttpResponse> taskCompletion;

		public bool IsDisposed { get; private set; }

		internal DispatchInfo(HttpRequest request)
		{
			this.request = request;
			this.taskCompletion = new TaskCompletionSource<HttpResponse>();
			this.cancellationTokenSource = new CancellationTokenSource();
			this.cancellationToken = cancellationTokenSource.Token;
			this.IsDisposed = false;
		}

		internal void SetResult(HttpResponse response)
		{
			taskCompletion.SetResult(response);
		}

		public void Dispose()
		{
			taskCompletion.SetCanceled();
			cancellationTokenSource.Cancel();
            if(!IsDisposed) {
				IsDisposed = true;
			}
		}
	}
}
