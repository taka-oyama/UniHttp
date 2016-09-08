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
	internal class ResponseBuilder : IDisposable
	{
		const char LF = '\n';

		StreamReader reader;
		MemoryStream messageBodyStream;
		System.Diagnostics.Stopwatch stopwatch;
		HttpResponse response;

		public ResponseBuilder(HttpRequest request, Stream sourceStream, int bufferSize = 1024)
		{
			this.reader = new StreamReader(sourceStream, bufferSize);
			this.messageBodyStream = new MemoryStream(0);
			this.stopwatch = new System.Diagnostics.Stopwatch();
			this.response = new HttpResponse(request);
		}

		public HttpResponse Build()
		{
			stopwatch.Start();

			ReadStatusLine();
			ReadHeaders();
			ReadMessageBody();

			stopwatch.Stop();
			response.RoundTripTime = stopwatch.Elapsed;

			return response;
		}

		void ReadStatusLine()
		{
			string line = reader.ReadTo(LF).Trim();
			string[] sliced = line.Split(' ');
			response.HttpVersion = sliced[0];
			response.StatusCode = int.Parse(sliced[1]);
			response.StatusPhrase = string.Join(" ", sliced.Skip(2).ToArray());
		}

		void ReadHeaders()
		{
			string name = reader.ReadTo(':', LF).Trim();
			while(name != String.Empty) {
				string valuesStr = reader.ReadTo(LF).Trim();
				string[] values = valuesStr.Split(';');
				foreach(string value in values) {
					response.Headers.Append(name, value);
				}
				name = reader.ReadTo(':', LF).Trim();
			}
		}

		void ReadMessageBody()
		{
			var bodyReader = new MessageBodyReader(reader);
			var decompress = response.Headers.Exist("Content-Encoding") && response.Headers["Content-Encoding"].Contains("gzip");

			if(response.Headers.Exist("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")) {
				bodyReader.ReadChunks(messageBodyStream, decompress);
				response.MessageBody = messageBodyStream.ToArray();
				return;
			}

			if(response.Headers.Exist("Content-Length")) {
				int length = int.Parse(response.Headers["Content-Length"][0]);
				bodyReader.Read(messageBodyStream, length, decompress);
				response.MessageBody = messageBodyStream.ToArray();
				return;
			}
		}

		public void Dispose()
		{
			reader.Dispose();
		}
	}
}
