using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace UniHttp
{
	public sealed class HttpRequest
	{
		public HttpMethod Method { get; private set; } 
		public Uri Uri { get; private set; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; private set; }
		public IHttpData Data { get; private set; }

		public HttpRequest(HttpMethod method, Uri uri, RequestHeaders headers = null, IHttpData data = null)
		{
			this.Method = method;
			this.Uri = uri;
			this.Headers = headers ?? new RequestHeadersDefaultBuilder(this).Build();
			this.Data = data;
		}

		public byte[] ToBytes()
		{
			List<byte> bytes = new List<byte>();
			bytes.AddRange(Encoding.UTF8.GetBytes(ConstructHeader()));
			if(Data != null) {
				bytes.AddRange(Data.ToBytes());
			}
			return bytes.ToArray();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(ConstructHeader());
			if(Data != null) {
				sb.Append(Data.ToString());
			}
			return sb.ToString();
		}

		string ConstructHeader()
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
			sb.Append(Constant.CRLF);
			return sb.ToString();
		}
	}
}
