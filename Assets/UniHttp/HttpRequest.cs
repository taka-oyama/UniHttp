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

		public Action<HttpResponse> OnComplete;

		public HttpRequest(Uri uri, HttpMethod method, RequestHeaders headers = null)
		{
			this.Uri = uri;
			this.Method = method;
			this.Headers = headers ?? new RequestHeadersDefaultBuilder(this).Build();
		}

		public IDisposable Send(Action<HttpResponse> onComplete)
		{
			return new HttpConnection(this).Send(onComplete);
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
			return sb.ToString();
		}
	}
}
