using UnityEngine;

namespace UniHttp
{
	public class HttpSettings
	{
		public string dataDirectory;
		public int? maxConcurrentRequests;
		public int? maxPersistentConnections;

		public ILogger logger;
		public ISslVerifier sslVerifier;
		public IFileHandler fileHandler;

		internal void FillWithDefaults()
		{
			dataDirectory = dataDirectory ?? Application.temporaryCachePath;
			maxConcurrentRequests = maxConcurrentRequests ?? 4;
			maxPersistentConnections = maxPersistentConnections ?? 6;

			logger = logger ?? Debug.unityLogger;
			sslVerifier = sslVerifier ?? new DefaultSslVerifier();
			fileHandler = fileHandler ?? new DefaultFileHandler();
		}
	}
}
