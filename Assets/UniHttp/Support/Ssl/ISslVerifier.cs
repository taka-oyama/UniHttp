using UnityEngine;
using System.Net.Security;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace UniHttp
{
	public interface ISslVerifier
	{
		bool Verify(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
	}
}
