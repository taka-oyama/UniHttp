using UnityEngine;

namespace UniHttp
{
	public struct HttpSetting
	{
		public bool useCookies;
		public bool useCache;
		public bool followRedirects;

		public static HttpSetting Default
		{
			get {
				return new HttpSetting() {
					useCookies = true,
					useCache = true,
					followRedirects = true,
				};
			}
		}
	}
}
