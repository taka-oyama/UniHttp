using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net.Security;

namespace UniHttp
{
	internal sealed class HttpStream : Stream
	{
		internal string url;
		TcpClient tcpClient;
		SslStream sslStream;
		Stream stream;

		internal HttpStream(Uri uri)
		{
			this.url = string.Concat(uri.Scheme, Uri.SchemeDelimiter, uri.Authority); 
			this.tcpClient = new TcpClient();
			this.tcpClient.Connect(uri.Host, uri.Port);

			this.stream = tcpClient.GetStream();

			if(uri.Scheme == Uri.UriSchemeHttp) {
				return;
			}

			if(uri.Scheme == Uri.UriSchemeHttps) {
				this.sslStream = new SslStream(stream, false, HttpManager.SslVerifier.Verify);
				this.stream = sslStream;
				sslStream.AuthenticateAsClient(uri.Host);
				return;
			}

			throw new Exception("Unsupported Scheme:" + uri.Scheme);
		}

		public bool Connected
		{
			get { return tcpClient.Connected; }
		}

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return stream.CanTimeout; }
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, 0, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		public override void Flush()
		{
			stream.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing) {
				stream.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void Close()
		{
			Dispose(true);
			base.Close();
		}
	}
}
