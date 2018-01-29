﻿using System;
using System.Net.Sockets;
using System.Net.Security;
using UnityEngine;

namespace UniHttp
{
	internal sealed class HttpStream : BaseStream
	{
		readonly internal string baseUrl;
		readonly internal KeepAlive keepAlive;
		readonly Uri uri;
		readonly TcpClient tcpClient;
		readonly ISslVerifier sslVerifier;

		internal HttpStream(Uri uri, HttpSettings settings) : base(null)
		{
			this.baseUrl = string.Concat(uri.Scheme, Uri.SchemeDelimiter, uri.Authority);
			this.keepAlive = new KeepAlive(DateTime.Now + settings.keepAliveTimeout);
			this.uri = uri;
			this.tcpClient = new TcpClient();
			this.sslVerifier = settings.sslVerifier;
			this.tcpClient.NoDelay = settings.tcpNoDelay;
			this.tcpClient.SendTimeout = (int) settings.tcpSendTimeout.TotalMilliseconds;
			this.tcpClient.ReceiveTimeout = (int) settings.tcpReceiveTimeout.TotalMilliseconds;
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
	}
}
