using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniHttp
{
	internal abstract class BaseStream : Stream
	{
		protected Stream stream;
		protected int bufferSize;

		internal BaseStream(Stream stream, int bufferSize = 64 * 1024)
		{
			this.stream = stream;
			this.bufferSize = bufferSize;
		}

		public long BytesRemaining => stream.Length - stream.Position;	

		public override bool CanRead => stream.CanRead;

		public override bool CanSeek => stream.CanSeek;

		public override bool CanTimeout => stream.CanTimeout;

		public override bool CanWrite => stream.CanTimeout;

		public override long Length => stream.Length;

		public override long Position
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override int ReadTimeout
		{
			get { return stream.ReadTimeout; }
			set { stream.ReadTimeout = value; }
		}

		public override int WriteTimeout
		{
			get { return stream.WriteTimeout; }
			set { stream.WriteTimeout = value; }
		}

		public override void Close()
		{
			stream?.Close();
			base.Close();
		}

		public async override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			await stream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
		}

		public async Task CopyToAsync(Stream destination, long count, CancellationToken cancellationToken, Progress progress = null)
		{
			byte[] buffer = new byte[bufferSize];
			long remainingBytes = count;
			int readBytes = 0;
			while(remainingBytes > 0) {
				readBytes = await ReadAsync(buffer, 0, (int)Math.Min(buffer.LongLength, remainingBytes), cancellationToken);
				await destination.WriteAsync(buffer, 0, readBytes, cancellationToken).ConfigureAwait(false);
				remainingBytes -= readBytes;
				progress?.Report(progress.Current + readBytes);
			}
		}

		public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
		{
			byte[] buffer = new byte[bufferSize];
			int readBytes = 0;
			while((readBytes = await ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0) {
				await destination.WriteAsync(buffer, 0, readBytes, cancellationToken).ConfigureAwait(false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing) {
				stream?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public async override Task FlushAsync(CancellationToken cancellationToken)
		{
			await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return await stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
		}

		public override int ReadByte()
		{
			return stream.ReadByte();
		}

		public string ReadTo(params char[] stoppers)
		{
			return ReadTo(new MemoryStream(), stoppers);
		}

		public string ReadTo(MemoryStream destination, params char[] stoppers)
		{
			int readByte;
			while(true) {
				readByte = ReadByte();
				if(readByte == -1) {
					break;
				}
				destination.WriteByte((byte)readByte);
				foreach(char stopper in stoppers) {
					if(readByte == stopper) {
						return Encoding.UTF8.GetString(destination.ToArray());
					}
				}
			}
			return null;
		}

		public async Task<string> ReadToAsync(CancellationToken cancellationToken, params char[] stoppers)
		{
			return await ReadToAsync(new MemoryStream(), cancellationToken, stoppers);
		}

		public async Task<string> ReadToAsync(MemoryStream destination, CancellationToken cancellationToken, params char[] stoppers)
		{
			byte[] buffer = new byte[1];
			while(true) {
				await ReadAsync(buffer, 0, buffer.Length, cancellationToken);
				destination.WriteByte(buffer[0]);
				foreach(char stopper in stoppers) {
					if(buffer[0] == stopper) {
						return Encoding.UTF8.GetString(destination.ToArray());
					}
				}
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		public async Task SendToAsync(Stream source, CancellationToken cancellationToken, Progress progress = null)
		{
			byte[] buffer = new byte[bufferSize];
			int writeSize = 0;
			while((writeSize = await source.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false)) != 0) {
				await WriteAsync(buffer, 0, writeSize, cancellationToken);
				progress?.Report(progress.Current + writeSize);
			}
		}

		public void SkipTo(params char[] stoppers)
		{
			int readByte;
			while(true) {
				readByte = ReadByte();
				if(readByte == -1) {
					break;
				}
				foreach(char stopper in stoppers) {
					if(readByte == stopper) {
						return;
					}
				}
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			await stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
		}

		public override void WriteByte(byte value)
		{
			stream.WriteByte(value);
		}
	}
}
