﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UniRx;
using UnityEngine;

namespace UniHttp
{
	public sealed class HttpRequest
	{
		public HttpMethod Method { get; private set; } 
		public Uri Uri { get; private set; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; private set; }
		public IHttpData Data { get; private set; } 

		public HttpRequest(Uri uri, HttpMethod method, RequestHeaders headers = null, IHttpData data = null)
		{
			this.Uri = uri;
			this.Method = method;
			this.Headers = headers ?? new RequestHeadersDefaultBuilder(this).Build();
			this.Data = data;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Method.ToString().ToUpper());
			sb.Append(Constant.SPACE);
			sb.Append(Uri.PathAndQuery);
			sb.Append(Constant.SPACE);
			sb.Append("HTTP/" + Version);
			sb.Append(Constant.CRLF);
			sb.Append(Headers.ToString());
			sb.Append(Constant.CRLF);
			if(Data != null) {
				sb.Append(Constant.CRLF);
				sb.Append(HttpManager.JsonSerializer.Serialize(Data));
				sb.Append(Constant.CRLF);
			}
			sb.Append(Constant.CRLF);
			return sb.ToString();
		}
	}
}
