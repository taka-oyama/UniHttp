using System;
using System.Net.Sockets;
using System.Net.Security;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace UniHttp
{
	internal sealed class HttpStream : BaseStream
	{
		internal string BaseUrl { get; private set; }
		internal KeepAlive KeepAlive { get; private set; }
		internal bool Connected { get; private set; }
		readonly Uri uri;
		readonly TcpClient tcpClient;
		ISslVerifier sslVerifier;

		internal HttpStream(Uri uri, ISslVerifier sslVerifier) : base(null)
		{
			this.BaseUrl = string.Concat(uri.Scheme, Uri.SchemeDelimiter, uri.Authority);
			this.KeepAlive = new KeepAlive();

			this.uri = uri;
			this.tcpClient = new TcpClient();
			this.sslVerifier = sslVerifier;
			this.Connected = false;
		}

		internal void UpdateSettings(HttpSettings settings)
		{
			KeepAlive.Reset(DateTime.Now + settings.KeepAliveTimeout.Value);
			tcpClient.NoDelay = settings.TcpNoDelay.Value;
			tcpClient.SendTimeout = (int)settings.TcpSendTimeout.Value.TotalMilliseconds;
			tcpClient.ReceiveTimeout = (int)settings.TcpReceiveTimeout.Value.TotalMilliseconds;
		}

		internal void Connect()
		{
			tcpClient.Connect(uri.Host, uri.Port);
			tcpClient.SendBufferSize = bufferSize;
			tcpClient.ReceiveBufferSize = bufferSize;

			stream = tcpClient.GetStream();
			if(uri.Scheme == Uri.UriSchemeHttps) {
				SslStream sslStream = new SslStream(stream, false, sslVerifier.Verify);
				sslStream.AuthenticateAsClient(uri.Host);
				stream = sslStream;
			}
			Connected = true;
		}

		internal async Task ConnectAsync()
		{
			await tcpClient.ConnectAsync(uri.Host, uri.Port).ConfigureAwait(false);
			tcpClient.SendBufferSize = bufferSize;
			tcpClient.ReceiveBufferSize = bufferSize;

			stream = tcpClient.GetStream();

			if(uri.Scheme == Uri.UriSchemeHttps) {
				SslStream sslStream = new SslStream(stream, false, sslVerifier.Verify);
				await sslStream.AuthenticateAsClientAsync(uri.Host).ConfigureAwait(false);
				stream = sslStream;
			}
			Connected = true;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing) {
				Connected = false;
			}
			base.Dispose(disposing);
		}
	}
}
