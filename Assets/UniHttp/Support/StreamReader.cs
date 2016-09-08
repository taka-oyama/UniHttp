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
		Stream networkStream;
		int bufferSize;

		internal StreamReader(Stream networkStream, int bufferSize = 1024)
		{
			this.networkStream = networkStream;
			this.bufferSize = bufferSize;
		}

		internal byte[] Read(int totalBytes)
		{
			using(MemoryStream dataStream = new MemoryStream(totalBytes)) {
				byte[] buffer = new byte[bufferSize];
				int readBytes = 0;
				while(totalBytes > 0) {
					readBytes = networkStream.Read(buffer, 0, Math.Min(buffer.Length, totalBytes));
					dataStream.Write(buffer, 0, readBytes);
					totalBytes -= readBytes;
				}
				return dataStream.ToArray();
			}
		}

		internal byte[] ReadAndDecompress(int totalBytes)
		{
			using(MemoryStream dataStream = new MemoryStream(0)) {
				using(GZipStream gzipStream = new GZipStream(networkStream, CompressionMode.Decompress)) {
					byte[] buffer = new byte[bufferSize];
					int readBytes = 0;
					while(totalBytes > 0) {
						while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
							dataStream.Write(buffer, 0, readBytes);
							totalBytes -= readBytes;
						}
					}
					return dataStream.ToArray();
				}
			}
		}
			
		internal string ReadUpTo(params char[] stoppers)
		{
			using(MemoryStream dataStream = new MemoryStream(0)) {
				int b = networkStream.ReadByte();
				while(stoppers.All(s => b != (int)s) && b != -1) {
					dataStream.WriteByte((byte)b);
					b = networkStream.ReadByte();
				}
				return Encoding.UTF8.GetString(dataStream.ToArray()).Trim();
			}
		}

		public void Dispose()
		{
			networkStream.Flush();
		}
	}
}
