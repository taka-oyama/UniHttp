using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpContext : HttpSettings
	{
		public int MaxConcurrentRequests = 4;
		public string DataDirectory;
		public ISslVerifier SslVerifier;
		public IFileHandler FileHandler;
		public ILogger Logger;

		internal HttpContext FillWithDefaults()
		{
			DataDirectory = DataDirectory ?? Application.temporaryCachePath;
			SslVerifier = SslVerifier ?? new DefaultSslVerifier();
			FileHandler = FileHandler ?? new DefaultFileHandler();
			Logger = Logger ?? Debug.unityLogger;

			FillWith(new HttpSettings() {
				AppendUserAgentToRequest = true,
				AllowCompressedResponse = true,
				FollowRedirects = true,
				Proxy = null,
				TcpNoDelay = true,
				TcpReceiveTimeout = TimeSpan.FromSeconds(30),
				TcpSendTimeout = TimeSpan.FromSeconds(30),
				KeepAliveTimeout = TimeSpan.FromSeconds(10f),
				UseCookies = true,
				UseCache = true,
			});

			return this;
		}
	}
}
