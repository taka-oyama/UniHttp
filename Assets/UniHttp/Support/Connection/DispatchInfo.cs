using UnityEngine;
using System;

namespace UniHttp
{
	internal class DispatchInfo
	{
		internal HttpRequest Request { get; private set; }
		internal Action<HttpResponse> OnResponse { get; private set; }

		internal DispatchInfo(HttpRequest request, Action<HttpResponse> onResponse)
		{
			this.Request = request;
			this.OnResponse = onResponse;
		}
	}
}
