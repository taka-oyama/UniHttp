using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpSettings
	{
		public bool? appendUserAgentToRequest;
		public bool? allowCompressedResponse;
		public bool? followRedirects;
		public HttpProxy proxy;
		public bool? tcpNoDelay;
		public TimeSpan? tcpReceiveTimeout;
		public TimeSpan? tcpSendTimeout;
		public TimeSpan? keepAliveTimeout;
		public bool? useCookies;
		public bool? useCache;

		internal void FillWith(HttpSettings source)
		{
			allowCompressedResponse = allowCompressedResponse ?? source.allowCompressedResponse;
			appendUserAgentToRequest = appendUserAgentToRequest ?? source.appendUserAgentToRequest;
			followRedirects = followRedirects ?? source.followRedirects;
			keepAliveTimeout = keepAliveTimeout ?? source.keepAliveTimeout;
			proxy = proxy ?? source.proxy;
			tcpNoDelay = tcpNoDelay ?? source.tcpNoDelay;
			tcpReceiveTimeout = tcpReceiveTimeout ?? source.tcpReceiveTimeout;
			tcpSendTimeout = tcpSendTimeout ?? source.tcpSendTimeout;
			useCache = useCache ?? source.useCache;
			useCookies = useCookies ?? source.useCookies;
		}
	}
}
