using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using UnityEngine;
using System.Threading;
using System.Globalization;

namespace UniHttp
{
	internal class HttpResponseParser : IDisposable
	{
		const char LF = '\n';

		NetworkStream inputStream;
		HttpResponse response;
		SynchronizationContext context;
		System.Timers.Timer timer;
		int bufferSize;

		public HttpResponseParser(HttpRequest request, NetworkStream inputStream, int bufferSize)
		{
			this.inputStream = inputStream;
			this.context = SynchronizationContext.Current;
			this.response = new HttpResponse(request);
			this.bufferSize = bufferSize;
			this.timer = new System.Timers.Timer(10.0);
			timer.AutoReset = false;
			timer.Elapsed += EachTimer;
		}

		public void Parse(Action<HttpResponse> doneCallback)
		{
			ReadStatusLine(() => {
				ReadHeaders(() => {
					ReadMessageBody(() => {
						doneCallback(response);
					});
				});
			});
		}

		void ReadStatusLine(Action nextAction)
		{
			ReadTo(LF, line => {
				string[] sliced = line.Split(' ');
				response.HttpVersion = sliced[0];
				response.StatusCode = int.Parse(sliced[1]);
				response.StatusPhrase = string.Join(" ", sliced.Skip(2).ToArray());
				nextAction();
			});
		}

		void ReadHeaders(Action nextAction)
		{
			ReadTo(new char[] { ':', LF }, name => {
				if(name != String.Empty) {
					ReadTo(LF, valuesStr => {
						string[] values = valuesStr.Split(';');
						foreach(string value in values) {
							response.Headers.Append(name, value);
						}
						ReadHeaders(nextAction);
					});
				} else {
					nextAction();
				}
			});
		}

		void ReadTo(char stopper, Action<string> doneCallback)
		{
			ReadTo(new char [] { stopper }, doneCallback);
		}

		void ReadTo(char[] stoppers, Action<string> doneCallback)
		{
			ScheduleRead(
				streamSize: 0,
				readAction: tempStream => {
					int b = inputStream.ReadByte();
					while(stoppers.All(s => b != (int)s) && b != -1) {
						tempStream.WriteByte((byte)b);
						b = inputStream.ReadByte();
					}
					return false;
				},
				doneCallback: bytes => {
					doneCallback(Encoding.UTF8.GetString(bytes).Trim());
				}
			);
		}

		void ReadMessageBody(Action nextAction)
		{
			if(response.Headers ["Transfer-Encoding"].Contains("chunked")) {
				ReadMessageBodyWithChunk(nextAction);
			} else if(response.Headers.Exist("content-length")) {
				int length = int.Parse(response.Headers["content-length"][0]);
				ReadMessageBodyWithLength(length, nextAction);
			}
		}

		void ReadMessageBodyWithChunk(Action nextAction)
		{
			int readBytes = 0;
			byte[] buffer = new byte[bufferSize];
			MemoryStream tempStream = new MemoryStream(0);
			ReadTo(LF, msg => {
				int nextBytes = int.Parse(msg, NumberStyles.HexNumber);
			});
		}

		void ReadMessageBodyWithLength(int totalBytes, Action nextAction)
		{
			int readBytes = 0;
			byte[] buffer = new byte[bufferSize];

			this.readAction = readAction;
			this.doneCallback = doneCallback;
			timer.Start();

			ScheduleRead(
				streamSize: totalBytes,
				readAction: tempStream => {
					readBytes = inputStream.Read(buffer, 0, buffer.Length);
					totalBytes-= readBytes;
					tempStream.Write(buffer, 0, readBytes);
					return totalBytes > 0;
				}, doneCallback: bytes => {
					Debug.Log(Encoding.UTF8.GetString(bytes));
					Debug.Log(bytes.Length);
					response.MessageBody = bytes;
					nextAction();
				}
			);
		}

		MemoryStream tempStream;
		Func<MemoryStream, bool> readAction;
		Action<byte[]> doneCallback;

		void ScheduleRead(int streamSize, Func<MemoryStream, bool> readAction, Action<byte[]> doneCallback)
		{
			this.tempStream = new MemoryStream(streamSize);
			this.readAction = readAction;
			this.doneCallback = doneCallback;
			timer.Start();
		}

		void EachTimer(object sender, ElapsedEventArgs evt)
		{
			try {
				timer.Stop();
				if(readAction(tempStream)) {
					timer.Start();
				} else {
					timer.Stop();
					tempStream.Dispose();
					doneCallback(tempStream.ToArray());
				}
			}
			catch(Exception exception) {
				context.Send(e => {
					Dispose();
					tempStream.Dispose();
					throw exception;
				}, null);
			}
		}

		public void Dispose()
		{
			inputStream.Flush();
			timer.Stop();
			timer.Dispose();
		}
	}
}
