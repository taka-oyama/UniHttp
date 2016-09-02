using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using UnityEngine;
using System.Threading;

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
					return true;
				},
				doneCallback: bytes => {
					doneCallback(Encoding.UTF8.GetString(bytes).Trim());
				}
			);
		}

		void ReadMessageBody(Action nextAction)
		{
			int contentLength = 0;
			int readBytes = 0;
			byte[] buffer = new byte[bufferSize];

			if(response.Headers.Exist("content-length")) {
				contentLength = int.Parse(response.Headers["content-length"][0]);
			}

			ScheduleRead(
				streamSize: contentLength,
				readAction: tempStream => {
					readBytes = inputStream.Read(buffer, 0, buffer.Length);
					if(readBytes > 0) {
						tempStream.Write(buffer, 0, readBytes);
						return true;
					} else {
						return false;
					}
				}, doneCallback: bytes => {
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
				if(inputStream.DataAvailable) {
					timer.Stop();
					if(readAction(tempStream)) {
						tempStream.Dispose();
						doneCallback(tempStream.ToArray());
					} else {
						timer.Start();
					}
				} else {
					timer.Start();
				}
			}
			catch(Exception exception) {
				Debug.Log(exception.ToString());
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
