using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UniRx;
using UnityEngine;

namespace UniHttp
{
	public class HttpRequest
	{
		public HttpMethod Method { get; private set; } 
		public Uri Uri { get; private set; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; private set; }
		public object Payload { get; private set; } 

		public HttpRequest(Uri uri, HttpMethod method, RequestHeaders headers = null, object payload = null)
		{
			this.Uri = uri;
			this.Method = method;
			this.Headers = headers ?? new RequestHeadersDefaultBuilder(this).Build();
			this.Payload = payload;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Method.ToString().ToUpper());
			sb.Append(" ");
			sb.Append(Uri.ToString());
			sb.Append("\n");
			sb.Append(Headers.ToString());
			sb.Append("\n");
			if(Payload != null) {
				sb.Append("\n");
				sb.Append(HttpDispatcher.JsonSerializer.Serialize(Payload));
				sb.Append("\n");
			}
			return sb.ToString();
		}
	}
}
