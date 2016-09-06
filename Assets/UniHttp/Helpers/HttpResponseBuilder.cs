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
	internal class HttpResponseBuilder : IDisposable
	{
		const char LF = '\n';

		HttpStreamReader reader;
		HttpResponse response;

		public HttpResponseBuilder(HttpRequest request, NetworkStream networkStream, int bufferSize)
		{
			this.response = new HttpResponse(request);
			this.reader = new HttpStreamReader(networkStream, bufferSize);
		}

		public HttpResponse Parse()
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
			if(name != String.Empty) {
				string valuesStr = reader.ReadUpTo(LF).Trim();
				string[] values = valuesStr.Split(';');
				foreach(string value in values) {
					response.Headers.Append(name, value);
				}
				ReadHeaders();
			}
		}

		void ReadMessageBody()
		{
			if(response.Headers.Exist("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")) {
				ReadMessageBodyWithChunk();
				return;
			}
			if(response.Headers.Exist("Content-Length")) {
				int length = int.Parse(response.Headers["Content-Length"][0]);
				ReadMessageBodyWithLength(length);
				return;
			}
			throw new Exception("Bad Response from server. Check for incorrent Transfer-Encoding.");
		}

		void ReadMessageBodyWithChunk()
		{
			MemoryStream chunk = new MemoryStream(0);
			int chunkSize = NextChunkSize();
			while(chunkSize > 0) {
				byte[] bytes = reader.Read(chunkSize);
				chunk.Write(bytes, 0, bytes.Length);
				chunkSize = NextChunkSize();
			}
			response.MessageBody = chunk.ToArray();
		}

		int NextChunkSize()
		{
			string hexStr = reader.ReadUpTo(LF).Trim();
			return int.Parse(hexStr, NumberStyles.HexNumber);
		}

		void ReadMessageBodyWithLength(int totalBytes)
		{
			response.MessageBody = reader.Read(totalBytes);
		}

		public void Dispose()
		{
			reader.Dispose();
		}
	}
}
