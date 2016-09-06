using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.IO;
using System.Timers;
using System.Threading;
using UniRx;

namespace UniHttp
{
	public class HttpStreamReader : IDisposable
	{
		NetworkStream networkStream;
		int bufferSize;

		internal HttpStreamReader(NetworkStream networkStream, int bufferSize = 1024 * 1024)
		{
			this.networkStream = networkStream;
			this.bufferSize = bufferSize;
		}

		internal byte[] Read(int totalBytes)
		{
			byte[] buffer = new byte[bufferSize];
			MemoryStream dataStream = new MemoryStream(totalBytes);
			int readBytes = 0;
			if(totalBytes > 0) {
				if(networkStream.DataAvailable) {
					readBytes = networkStream.Read(buffer, 0, buffer.Length);
					totalBytes -= readBytes;
					dataStream.Write(buffer, 0, readBytes);
				} else {
					Thread.Sleep(TimeSpan.FromMilliseconds(1));
				}
			}
			return dataStream.ToArray();
		}

		internal string ReadUpTo(params char[] stoppers)
		{
			while(!networkStream.DataAvailable) {
				Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			}
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
