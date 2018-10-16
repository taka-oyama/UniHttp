using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpSettings
	{
		public bool? AppendUserAgentToRequest;
		public bool? AllowCompressedResponse;
		public bool? FollowRedirects;
		public HttpProxy Proxy;
		public bool? TcpNoDelay;
		public TimeSpan? TcpReceiveTimeout;
		public TimeSpan? TcpSendTimeout;
		public TimeSpan? KeepAliveTimeout;
		public bool? UseCookies;
		public bool? UseCache;

		internal void FillWith(HttpSettings source)
		{
			AllowCompressedResponse = AllowCompressedResponse ?? source.AllowCompressedResponse;
			AppendUserAgentToRequest = AppendUserAgentToRequest ?? source.AppendUserAgentToRequest;
			FollowRedirects = FollowRedirects ?? source.FollowRedirects;
			KeepAliveTimeout = KeepAliveTimeout ?? source.KeepAliveTimeout;
			Proxy = Proxy ?? source.Proxy;
			TcpNoDelay = TcpNoDelay ?? source.TcpNoDelay;
			TcpReceiveTimeout = TcpReceiveTimeout ?? source.TcpReceiveTimeout;
			TcpSendTimeout = TcpSendTimeout ?? source.TcpSendTimeout;
			UseCache = UseCache ?? source.UseCache;
			UseCookies = UseCookies ?? source.UseCookies;
		}
	}
}
