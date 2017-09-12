using UnityEngine;

namespace UniHttp
{
	public class HttpSettings
	{
		public bool useCookies = true;
		public bool useCache = true;
		public bool followRedirects = true;
		public int maxConcurrentRequests = 4;
		public int maxPersistentConnections = 6;

		public string dataDirectory;
		public ILogger logger;
		public ISslVerifier sslVerifier;
		public IFileHandler fileHandler;

		internal HttpSettings FillWithDefaults()
		{
			dataDirectory = dataDirectory ?? Application.temporaryCachePath;
			logger = logger ?? Debug.unityLogger;
			sslVerifier = sslVerifier ?? new DefaultSslVerifier();
			fileHandler = fileHandler ?? new DefaultFileHandler();

			return this;
		}
	}
}
