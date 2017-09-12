using UnityEngine;

namespace UniHttp
{
	internal static class Constant
	{
		internal const string SPACE = " ";
		internal const string CRLF = "\r\n";

		internal static int[] REDIRECTS = new [] {301, 302, 303, 307, 308};
	}
}
