using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace UniHttp
{
	internal class HttpConnection : IDisposable
	{
		HttpRequest request;
		HttpStream stream;

		internal HttpConnection(HttpRequest request)
		{
			this.request = request;
		}

		internal HttpResponse Send()
		{
			try {
				return Transmit();
			}
			catch(SocketException exception) {
				return BuildErrorResponse(exception);
			}
			finally {
				Dispose();
			}
		}

		HttpResponse Transmit()
		{
			TcpClient tcpClient = new TcpClient();
			tcpClient.Connect(request.Uri.Host, request.Uri.Port);

			this.stream = new HttpStream(tcpClient, request.Uri);

			byte[] data = new RequestDataBuilder(request).Build();
			stream.Write(data, 0, data.Length);
			stream.Flush();

			HttpResponse response = new ResponseBuilder(request, stream).Build();
			if(response.StatusCode == 301) {
				
			}

			Dispose();
			return response;
		}

		HttpResponse BuildErrorResponse(Exception exception)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "Unknown";
			response.StatusCode = 0;
			response.StatusPhrase = exception.Message.Trim();
			response.MessageBody = Encoding.UTF8.GetBytes(exception.StackTrace);
			return response;
		}

		public void Dispose()
		{
			if(stream != null) {
				stream.Dispose();
			}
		}
	}
}
