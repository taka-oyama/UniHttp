using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net.Security;
using System.Threading;

namespace UniHttp
{
	public class HttpStream : Stream
	{
		SslClient sslClient;
		Stream stream;

		public HttpStream(TcpClient tcpClient, Uri uri)
		{
			this.stream = tcpClient.GetStream();

			if(uri.Scheme == Uri.UriSchemeHttp) {
				return;
			}
			if(uri.Scheme == Uri.UriSchemeHttps) {
				this.sslClient = new SslClient(uri, stream, true);
				this.stream = sslClient.Authenticate(SslClient.NoVerify);
				return;
			}
			throw new Exception("Unsupported Scheme:" + uri.Scheme);
		}

		public override bool CanRead { get { return stream.CanRead; } }
		public override bool CanSeek { get { return stream.CanSeek; } }
		public override bool CanWrite { get { return stream.CanTimeout; } }
		public override long Length { get { return stream.Length; } }
		public override long Position {
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
				if(sslClient != null) {
					sslClient.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
