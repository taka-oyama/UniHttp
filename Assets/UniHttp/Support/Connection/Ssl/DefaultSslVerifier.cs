using UnityEngine;
using System.Net.Security;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace UniHttp
{
	public class DefaultSslVerifier : ISslVerifier
	{
		public bool Verify(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if(DateTime.Parse(certificate.GetExpirationDateString()) <= DateTime.Now) {
				return false;
			}

			Debug.Log(sslPolicyErrors.ToString());
			if(sslPolicyErrors == SslPolicyErrors.None) {
				return true;
			}

			return false;
		}
	}
}
