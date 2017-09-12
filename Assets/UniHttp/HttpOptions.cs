using UnityEngine;

namespace UniHttp
{
	public struct HttpOptions
	{
		public bool useCookies;
		public bool useCache;
		public bool followRedirects;
		public int maxConcurrentRequests;

		public static HttpOptions Default
		{
			get {
				return new HttpOptions() {
					useCookies = true,
					useCache = true,
					followRedirects = true,
					maxConcurrentRequests = 4,
				};
			}
		}
	}
}
