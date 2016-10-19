using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

namespace UniHttp
{
	internal class HttpDispatcher
	{
		HttpRequest request;

		static int[] REDIRECTS = new [] {301, 302, 303, 307, 308};

		internal HttpDispatcher(HttpDispatchInfo info)
		{
			this.request = info.request;
		}

		internal HttpResponse SendWith(HttpStream stream)
		{
			try {
				return Transmit(stream);
			}
			catch(SocketException exception) {
				return BuildErrorResponse(exception);
			}
		}

		HttpResponse Transmit(HttpStream stream)
		{
			byte[] data = new RequestDataBuilder(request).Build();
			stream.Write(data, 0, data.Length);
			stream.Flush();

			HttpResponse response = new ResponseBuilder(request, stream).Build();
			// TODO: add redirection handling
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

		bool IsRedirect(HttpResponse response)
		{
			for(int i = 0; i < REDIRECTS.Length; i++) {
				if(response.StatusCode == REDIRECTS[i]) {
					return true;
				}
			}
			return false;
		}
	}
}
