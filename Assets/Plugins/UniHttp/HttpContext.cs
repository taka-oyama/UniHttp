using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpContext : HttpSettings
	{
		public int maxConcurrentRequests = 4;
		public string dataDirectory;
		public ISslVerifier sslVerifier;
		public IFileHandler fileHandler;
		public ILogger logger;

		internal HttpContext FillWithDefaults()
		{
			dataDirectory = dataDirectory ?? Application.temporaryCachePath;
			sslVerifier = sslVerifier ?? new DefaultSslVerifier();
			fileHandler = fileHandler ?? new DefaultFileHandler();
			logger = logger ?? Debug.unityLogger;

			FillWith(new HttpSettings() {
				appendUserAgentToRequest = true,
				allowCompressedResponse = true,
				followRedirects = true,
				proxy = null,
				tcpNoDelay = true,
				tcpReceiveTimeout = TimeSpan.FromSeconds(30),
				tcpSendTimeout = TimeSpan.FromSeconds(30),
				keepAliveTimeout = TimeSpan.FromSeconds(10f),
				useCookies = true,
				useCache = true,
			});

			return this;
		}
	}
}
