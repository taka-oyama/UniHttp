﻿using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpSettings
	{
		public bool useCookies = true;
		public bool useCache = true;
		public bool followRedirects = true;
		public int maxConcurrentRequests = 4;
		public TimeSpan keepAliveTimeout = TimeSpan.FromSeconds(10f);

		public int tcpBufferSize = 64 * 1024;
		public bool tcpNoDelay = true;

		public string dataDirectory;
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
