using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	public sealed class HttpRequest
	{
		public HttpMethod Method { get; private set; } 
		public Uri Uri { get; private set; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; private set; }
		public IHttpData Data { get; private set; }

		public HttpRequest(HttpMethod method, Uri uri) : this(method, uri, null, null, null) {}
		public HttpRequest(HttpMethod method, Uri uri, HttpQuery query) : this(method, uri, query, null, null) {}
		public HttpRequest(HttpMethod method, Uri uri, IHttpData data) : this(method, uri, null, null, data) {}
		public HttpRequest(HttpMethod method, Uri uri, RequestHeaders headers) : this(method, uri, null, headers, null) {}
		public HttpRequest(HttpMethod method, Uri uri, HttpQuery query, IHttpData data) : this(method, uri, query, null, data) {}
		public HttpRequest(HttpMethod method, Uri uri, RequestHeaders headers, IHttpData data) : this(method, uri, null, headers, data) {}
		public HttpRequest(HttpMethod method, Uri uri, HttpQuery query, RequestHeaders headers) : this(method, uri, query, headers, null) {}
		public HttpRequest(HttpMethod method, Uri uri, HttpQuery query, RequestHeaders headers, IHttpData data)
		{
			this.Method = method;
			this.Uri = ConstructUri(uri, query);
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

		Uri ConstructUri(Uri uri, HttpQuery query)
		{
			if(query == null) {
				return uri;
			}
			StringBuilder sb = new StringBuilder();
			sb.Append(uri.AbsoluteUri);
			sb.Append(uri.AbsolutePath);
			sb.Append("?");
			if(uri.Query.Length > 0) {
				sb.Append(uri.Query.Substring(1));
				sb.Append("&");
			}
			sb.Append(query.ToString());
			return new Uri(sb.ToString());
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
