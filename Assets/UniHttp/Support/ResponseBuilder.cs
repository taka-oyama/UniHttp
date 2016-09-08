﻿using System;
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
		HttpResponse response;

		public ResponseBuilder(HttpRequest request, Stream networkStream, int bufferSize = 1024)
		{
			this.response = new HttpResponse(request);
			this.reader = new StreamReader(networkStream, bufferSize);
		}

		public HttpResponse Build()
		{
			ReadStatusLine();
			ReadHeaders();
			ReadMessageBody();
			return response;
		}

		void ReadStatusLine()
		{
			string line = reader.ReadUpTo(LF).Trim();
			string[] sliced = line.Split(' ');
			response.HttpVersion = sliced[0];
			response.StatusCode = int.Parse(sliced[1]);
			response.StatusPhrase = string.Join(" ", sliced.Skip(2).ToArray());
		}

		void ReadHeaders()
		{
			string name = reader.ReadUpTo(':', LF).Trim();
			while(name != String.Empty) {
				string valuesStr = reader.ReadUpTo(LF).Trim();
				string[] values = valuesStr.Split(';');
				foreach(string value in values) {
					response.Headers.Append(name, value);
				}
				name = reader.ReadUpTo(':', LF).Trim();
			}
		}

		void ReadMessageBody()
		{
			var bodyReader = new MessageBodyReader(reader);
			var decompress = response.Headers.Exist("Content-Encoding") && response.Headers["Content-Encoding"].Contains("gzip");

			if(response.Headers.Exist("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")) {
				response.MessageBody = bodyReader.ReadChunks(decompress);
				return;
			}

			if(response.Headers.Exist("Content-Length")) {
				int length = int.Parse(response.Headers["Content-Length"][0]);
				response.MessageBody = bodyReader.Read(length, decompress);
				return;
			}

			throw new Exception("Bad Response from server. Check for incorrent Transfer-Encoding.");
		}

		public void Dispose()
		{
			reader.Dispose();
		}
	}
}
