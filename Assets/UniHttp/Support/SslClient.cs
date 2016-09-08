using UnityEngine;
using System.Net.Security;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace UniHttp
{
	internal class SslClient : IDisposable
	{
		Uri uri;
		Stream networkStream;
		bool leaveInnerStreamOpen;

		SslStream sslStream;

		internal SslClient(Uri uri, Stream networkStream, bool leaveInnerStreamOpen = true)
		{
			this.uri = uri;
			this.networkStream = networkStream;
			this.leaveInnerStreamOpen = leaveInnerStreamOpen;
		}

		internal SslStream Authenticate(RemoteCertificateValidationCallback onValidate = null)
		{
			sslStream = new SslStream(networkStream, leaveInnerStreamOpen, onValidate ?? DefaultVerify);
			sslStream.AuthenticateAsClient(uri.Host);
			return sslStream;
		}

		internal static bool DefaultVerify(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

		internal static bool NoVerify(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			#if DEBUG
			return true;
			#else
			throw new Exception("Do not use this in production!");
			#endif
		}

		public void Dispose()
		{
			if(sslStream != null) {
				sslStream.Close();
			}
		}
	}
}
