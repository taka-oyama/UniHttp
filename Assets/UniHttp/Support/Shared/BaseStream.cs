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

		public long BytesRemaining
		{
			get { return stream.Length - stream.Position; }
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

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return stream.ReadAsync(buffer, offset, count, cancellationToken);
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return stream.WriteAsync(buffer, offset, count, cancellationToken);
		}

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return stream.FlushAsync(cancellationToken);
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
			stream.Close();
			base.Close();
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

		public async Task CopyToAsync(Stream destination, long count, CancellationToken cancellationToken, Progress progress = null)
		{
			byte[] buffer = new byte[bufferSize];
			long remainingBytes = count;
			int readBytes = 0;
			while(remainingBytes > 0) {
				readBytes = await ReadAsync(buffer, 0, (int)Math.Min(buffer.LongLength, remainingBytes), cancellationToken).ConfigureAwait(false);
				await destination.WriteAsync(buffer, 0, readBytes, cancellationToken).ConfigureAwait(false);
				remainingBytes -= readBytes;

				if(progress != null) {
					progress.Report(progress.Read + readBytes);
				}
			}
		}

		public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
		{
			byte[] buffer = new byte[bufferSize];
			int readBytes = 0;
			while((readBytes = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
			{
				await destination.WriteAsync(buffer, 0, readBytes, cancellationToken);
			}
		}
	}
}
