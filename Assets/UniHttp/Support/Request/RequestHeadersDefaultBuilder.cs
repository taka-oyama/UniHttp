using UnityEngine;
using System;

namespace UniHttp
{	
	internal sealed class RequestHeadersDefaultBuilder
	{
		HttpRequest request;
		RequestHeaders headers;

		internal RequestHeadersDefaultBuilder(HttpRequest request)
		{
			this.request = request;
			this.headers = new RequestHeaders();
		}

		internal RequestHeaders Build()
		{
			headers.AddOrReplace("Accept-Encoding", "gzip");
			headers.AddOrReplace("Host", GenerateHost());
			headers.AddOrReplace("User-Agent", "UniHttp/1.0");
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
	}
}