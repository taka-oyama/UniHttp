using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace UniHttp
{
	public class HttpConnection
	{
		HttpRequest request;
		SslClient sslClient;

		internal HttpConnection(HttpRequest request)
		{
			this.request = request;
		}

		internal HttpResponse Send()
		{
			try {
				return Transmit();
			}
			catch(Exception exception) {
				return BuildErrorResponse(exception);
			}
			finally {
				Dispose();
			}
		}

		HttpResponse Transmit()
		{
			byte[] data = new RequestDataBuilder(request).Build();
			Stream networkStream = SetupStream();
			networkStream.Write(data, 0, data.Length);
			networkStream.Flush();
			HttpResponse response = new ResponseBuilder(request, networkStream).Build();
			Dispose();
			return response;
		}

		Stream SetupStream()
		{
			TcpClient socket = new TcpClient();
			socket.Connect(request.Uri.Host, request.Uri.Port);
			Stream stream = socket.GetStream();

			if(request.Uri.Scheme == Uri.UriSchemeHttp ) return stream;
			if(request.Uri.Scheme == Uri.UriSchemeHttps) return SetupSslStream(stream);
			throw new Exception("Unsupported Scheme:" + request.Uri.Scheme);
		}

		Stream SetupSslStream(Stream stream)
		{
			this.sslClient = new SslClient(request.Uri, stream, true);
			return sslClient.Authenticate(SslClient.NoVerify);
		}

		HttpResponse BuildErrorResponse(Exception exception)
		{
			HttpResponse response = new HttpResponse(request);
			response.HttpVersion = "";
			response.StatusCode = 0;
			response.StatusPhrase = exception.GetType().Name;
			response.MessageBody = Encoding.UTF8.GetBytes(exception.Message);
			return response;
		}

		public void Dispose()
		{
			if(sslClient != null) {
				sslClient.Dispose();
			}
		}
	}
}
