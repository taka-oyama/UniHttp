using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.IO;
using System.Timers;
using System.Threading;

namespace UniHTtp
{
	public class HttpByteReader
	{
		NetworkStream inputStream;
		System.Timers.Timer timer;
		SynchronizationContext context;
		MemoryStream tempStream;
		Func<bool> readAction;
		Action<byte[]> doneCallback;

		public HttpByteReader(NetworkStream inputStream)
		{
			this.inputStream = inputStream;
			this.context = SynchronizationContext.Current;
			this.timer = new System.Timers.Timer(1.0);
			timer.AutoReset = false;
			timer.Elapsed += EachTimer;
		}

		void Read(int totalBytes, int bufferSize, Action<string> readCallback)
		{
			byte[] buffer = new byte[bufferSize];

			this.tempStream = new MemoryStream(totalBytes);
			this.readAction = () => {
				int readBytes = inputStream.Read(buffer, 0, buffer.Length);
				totalBytes-= readBytes;
				tempStream.Write(buffer, 0, readBytes);
				return totalBytes > 0;
			};
			this.doneCallback = data => readCallback(Encoding.UTF8.GetString(data).Trim());

			timer.Start();
		}

		void ReadTo(char stopper, Action<string> doneCallback)
		{
			ReadTo(new char [] { stopper }, doneCallback);
		}

		void ReadTo(char[] stoppers, Action<string> readCallback)
		{
			if(timer.Enabled) throw new Exception("Read is still running");

			this.tempStream = new MemoryStream(0);
			this.readAction = () => ReadStreamUpTo(stoppers);
			this.doneCallback = data => readCallback(Encoding.UTF8.GetString(data).Trim());

			timer.Start();
		}

		bool ReadStreamUpTo(char[] stoppers)
		{
			int b = inputStream.ReadByte();
			while(stoppers.All(s => b != (int)s) && b != -1) {
				tempStream.WriteByte((byte)b);
				b = inputStream.ReadByte();
			}
			return false;
		}

		void EachTimer(object sender, ElapsedEventArgs evt)
		{
			try {
				timer.Stop();
				if(inputStream.DataAvailable) {
					if(readAction()) {
						timer.Start();
					} else {
						timer.Stop();
						tempStream.Dispose();
						context.Send(e => doneCallback(tempStream.ToArray()), null);
					}
				} else {
					timer.Start();
				}
			}
			catch(Exception exception) {
				context.Send(e => {
					Dispose();
					throw exception;
				}, null);
			}
		}

		public void Dispose()
		{
			if(tempStream != null) tempStream.Dispose();
			timer.Stop();
			timer.Dispose();
			inputStream.Flush();
		}
	}
}
