using UnityEngine;
using System;

namespace UniHttp
{
	internal struct HttpDispatchInfo
	{
		internal HttpRequest Request { get; private set; }
		internal Action<HttpResponse> Callback { get; private set; }

		internal HttpDispatchInfo(HttpRequest request, Action<HttpResponse> callback)
		{
			this.Request = request;
			this.Callback = callback;
		}
	}
}
