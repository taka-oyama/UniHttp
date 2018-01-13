using System;

namespace UniHttp
{
	internal struct CancellationToken
	{
		readonly DispatchInfo source;

		internal CancellationToken(DispatchInfo source)
		{
			this.source = source;
		}

		internal bool IsCancellationRequested
		{
			get { return source != null && source.IsDisposed; }
		}

		internal void ThrowCancellationException()
		{
			throw new OperationCanceledException(source.request.Uri + " was cancelled");
		}
	}
}