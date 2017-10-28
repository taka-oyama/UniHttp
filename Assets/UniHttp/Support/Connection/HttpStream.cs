﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using UnityEngine;

namespace UniHttp
{
	internal sealed class HttpStream : Stream
	{
		internal string baseUrl;
		internal KeepAlive keepAlive;

		Uri uri;
		TcpClient tcpClient;
		Stream stream;
		ISslVerifier sslVerifier;

		internal HttpStream(Uri uri, DateTime expiresAt, ISslVerifier sslVerifier)
		{
			this.baseUrl = string.Concat(uri.Scheme, Uri.SchemeDelimiter, uri.Authority);
			this.keepAlive = new KeepAlive(expiresAt);

			this.uri = uri;
			this.tcpClient = new TcpClient();
			this.sslVerifier = sslVerifier;
		}

		internal void Connect()
		{
			this.tcpClient.Connect(uri.Host, uri.Port);
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

		internal TcpClient TcpClient
		{
			get { return tcpClient; }
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
			return stream.Read(buffer, offset, count);
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

		public string ReadTo(params char[] stoppers)
		{
			using(MemoryStream destination = new MemoryStream()) {
				return ReadTo(destination, stoppers);
			}
		}

		public string ReadTo(MemoryStream destination, params char[] stoppers)
		{
			int b;
			int count = 0;
			do {
				b = ReadByte();
				destination.WriteByte((byte)b);
				if(b == -1) break;
				count += 1;
			}
			while(Array.TrueForAll(stoppers, s => b != (int)s));
			return Encoding.UTF8.GetString(destination.ToArray());
		}

		public void SkipTo(params char[] stopChars)
		{
			int b;
			int count = 0;
			do {
				b = ReadByte();
				if(b == -1) break;
				count += 1;
			}
			while(Array.TrueForAll(stopChars, s => b != (int)s));
		}

		public void CopyTo(Stream destination, long count, Progress progress = null)
		{
			byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
  			long remainingBytes = count;
			int readBytes = 0;
			while(remainingBytes > 0) {
				readBytes = Read(buffer, 0, (int) Math.Min(buffer.LongLength, remainingBytes));
				destination.Write(buffer, 0, readBytes);
				remainingBytes -= readBytes;

				if(progress != null) {
					progress.Report(progress.Read + readBytes);
				}
			}
		}
	}
}
