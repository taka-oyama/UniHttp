using UnityEngine;
using System;

namespace UniHttp
{	
	internal class RequestHeadersDefaultBuilder
	{
		HttpRequest request;
		RequestHeaders headers;

		static string appInfo = Application.bundleIdentifier + "/" + Application.version;
		static string osInfo = SystemInfo.operatingSystem;

		internal RequestHeadersDefaultBuilder(HttpRequest request)
		{
			this.request = request;
			this.headers = new RequestHeaders();
		}

		internal RequestHeaders Build()
		{
			headers.AddOrReplace("Accept-Encoding", "gzip");
			headers.AddOrReplace("Host", GenerateHost());
			headers.AddOrReplace("User-Agent", GenerateUserAgent());
			return headers;
		}

		string GenerateHost()
		{
			Uri uri = request.Uri;
			string host = uri.Host;
			if(uri.Scheme == Uri.UriSchemeHttp  && uri.Port != 80 || uri.Scheme == Uri.UriSchemeHttps && uri.Port != 443) {
				host += ":" + uri.Port; 
			}
			return host;
		}

		string GenerateUserAgent()
		{
			return string.Format("{0} ({1}) UniHttp/1.0", appInfo, osInfo);
		}
	}
}