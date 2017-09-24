using UnityEngine;

namespace UniHttp
{
	internal static class Constant
	{
		internal const string Space = " ";
		internal const string CRLF = "\r\n";

		internal static int[] Redirects = new [] {
			StatusCode.MovedPermanently,
			StatusCode.Found,
			StatusCode.SeeOther,
			StatusCode.TemporaryRedirected,
			StatusCode.PermanentRedirect,
		};
	}
}
