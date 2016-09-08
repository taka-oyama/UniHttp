using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.IO;
using System.Timers;
using System.Threading;
using Unity.IO.Compression;

namespace UniHttp
{
	public class StreamReader : IDisposable
	{
		Stream source;
		int bufferSize;

		internal StreamReader(Stream source, int bufferSize = 1024)
		{
			this.source = source;
			this.bufferSize = bufferSize;
		}

		internal void CopyTo(Stream destination, int totalBytes)
		{
			byte[] buffer = new byte[bufferSize];
			int readBytes = 0;
			while(totalBytes > 0) {
				readBytes = source.Read(buffer, 0, Math.Min(buffer.Length, totalBytes));
				destination.Write(buffer, 0, readBytes);
				totalBytes -= readBytes;
			}
		}

		internal void DecompressAndCopyTo(Stream destination, int totalBytes)
		{
			using(GZipStream gzipStream = new GZipStream(source, CompressionMode.Decompress)) {
				byte[] buffer = new byte[bufferSize];
				int readBytes = 0;
				while(totalBytes > 0) {
					while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
						destination.Write(buffer, 0, readBytes);
						totalBytes -= readBytes;
					}
				}
			}
		}

		internal string ReadTo(params char[] stoppers)
		{
			using(MemoryStream dataStream = new MemoryStream(0)) {
				int b = source.ReadByte();
				while(stoppers.All(s => b != (int)s) && b != -1) {
					dataStream.WriteByte((byte)b);
					b = source.ReadByte();
				}
				return Encoding.UTF8.GetString(dataStream.ToArray()).Trim();
			}
		}

		public void Dispose()
		{
			source.Flush();
		}
	}
}
