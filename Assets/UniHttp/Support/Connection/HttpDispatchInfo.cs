using UnityEngine;
using System;

namespace UniHttp
{
	internal struct HttpDispatchInfo
	{
		internal HttpRequest request;
		internal Action<HttpResponse> callback;

		internal HttpDispatchInfo(HttpRequest request, Action<HttpResponse> callback)
		{
			this.request = request;
			this.callback = callback;
		}
	}
}
