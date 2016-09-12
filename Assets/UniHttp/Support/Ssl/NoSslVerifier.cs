using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace UniHttp
{
	public class NoSslVerifier : ISslVerifier
	{
		public bool Verify(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			#if DEBUG
			return true;
			#else
			throw new Exception("Do not use this in production!");
			#endif
		}
	}
}
