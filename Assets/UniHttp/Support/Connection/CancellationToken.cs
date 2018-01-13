using System;

namespace UniHttp
{
	internal struct CancellationToken
	{
		readonly DispatchInfo source;

		public CancellationToken(DispatchInfo source)
		{
			this.source = source;
		}

		public bool IsCancellationRequested
		{
			get { return source != null && source.IsDisposed; }
		}

		public void ThrowCancellationException()
		{
			throw new OperationCanceledException(source.request.Uri + " was cancelled");
		}
	}
}