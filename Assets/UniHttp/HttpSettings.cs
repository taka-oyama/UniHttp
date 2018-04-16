using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpSettings
	{
		public bool appendDefaultUserAgentToRequest = true;
		public bool allowResponseCompression = true;
		public bool followRedirects = true;
		public bool tcpNoDelay = true;
		public TimeSpan tcpReceiveTimeout = TimeSpan.FromSeconds(30);
		public TimeSpan tcpSendTimeout = TimeSpan.FromSeconds(30);
		public bool useCookies = true;
		public bool useCache = true;

		public int maxConcurrentRequests = 4;
		public TimeSpan keepAliveTimeout = TimeSpan.FromSeconds(10f);

		public string dataDirectory;
		public HttpProxy proxy;
		public ISslVerifier sslVerifier;
		public IFileHandler fileHandler;
		public ILogger logger;

		internal HttpSettings FillWithDefaults()
		{
			this.dataDirectory = dataDirectory ?? Application.temporaryCachePath;
			this.sslVerifier = sslVerifier ?? new DefaultSslVerifier();
			this.fileHandler = fileHandler ?? new DefaultFileHandler();
			this.logger = logger ?? Debug.unityLogger;

			return this;
		}
	}
}
