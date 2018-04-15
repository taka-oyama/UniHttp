using System;
using UnityEngine;

namespace UniHttp
{
	public class WaitForResponse : CustomYieldInstruction, IDisposable
	{
		readonly internal DispatchInfo dispatchInfo;
		public HttpResponse response;

		internal WaitForResponse(HttpRequest request)
		{
			this.dispatchInfo = new DispatchInfo(request);
		}

		internal void SetResponse(HttpResponse _response)
		{
			this.response = _response;
		}

		public float progress
		{
			get { return dispatchInfo.downloadProgress.Ratio; }
		}

		public override bool keepWaiting
		{
			get { return response == null; }
		}

		public bool isDone
		{
			get { return !keepWaiting; }
		}

		public void Dispose()
		{
			dispatchInfo.Dispose();
		}
	}
}
