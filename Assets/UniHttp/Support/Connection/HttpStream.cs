using System;
using System.Net.Sockets;
using System.Net.Security;
using UnityEngine;

namespace UniHttp
{
	internal sealed class HttpStream : BaseStream
	{
		internal string baseUrl;
		internal KeepAlive keepAlive;
		Uri uri;
		TcpClient tcpClient;
		ISslVerifier sslVerifier;

		internal HttpStream(Uri uri, DateTime expiresAt, ISslVerifier sslVerifier, int bufferSize) : base(null, bufferSize)
		{
			this.baseUrl = string.Concat(uri.Scheme, Uri.SchemeDelimiter, uri.Authority);
			this.keepAlive = new KeepAlive(expiresAt);
			this.uri = uri;
			this.tcpClient = new TcpClient();
			this.sslVerifier = sslVerifier;
		}

		internal void Connect()
		{
			tcpClient.Connect(uri.Host, uri.Port);
			tcpClient.SendBufferSize = bufferSize;
			tcpClient.ReceiveBufferSize = bufferSize;

			this.stream = tcpClient.GetStream();

			if(uri.Scheme == Uri.UriSchemeHttps) {
				SslStream sslStream = new SslStream(stream, false, sslVerifier.Verify);
				sslStream.AuthenticateAsClient(uri.Host);
				this.stream = sslStream;
			}
		}

		internal bool Connected
		{
			get { return tcpClient.Connected; }
		}

		internal bool TcpNoDelay
		{
			get { return tcpClient.NoDelay; }
			set { tcpClient.NoDelay = value; }
		}
	}
}
