using System.Collections.Generic;
using System.Text;
using System;

namespace UniHttp
{
	public sealed class HttpRequest
	{
		public HttpMethod Method { get; }
		public Uri Uri { get; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; }
		public IHttpData Data { get; }
		public Progress Progress { get; }
		public HttpSettings Settings { get; }
		internal CacheMetadata Cache { get; set; }

		public HttpRequest(HttpMethod method, Uri uri) : this(method, uri, null, null) {}
		public HttpRequest(HttpMethod method, Uri uri, IHttpData data) : this(method, uri, null, data) {}
		public HttpRequest(HttpMethod method, Uri uri, HttpQuery query) : this(method, uri, query, null) { }
		public HttpRequest(HttpMethod method, Uri uri, HttpQuery query, IHttpData data)
		{
			this.Method = method;
			this.Uri = ConstructUri(uri, query);
			this.Headers = new RequestHeaders();
			this.Data = data;
			this.Progress = new Progress();
			this.Settings = new HttpSettings();
		}

		internal HttpRequest(Uri redirectUri, HttpRequest baseRequest)
		{
			this.Method = baseRequest.Method;
			this.Uri = redirectUri;
			this.Headers = baseRequest.Headers;
			this.Data = baseRequest.Data;
			this.Progress = baseRequest.Progress;
			this.Settings = baseRequest.Settings;
		}

		internal byte[] ToBytes()
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
			sb.Append(Constant.CRLF);
			return sb.ToString();
		}

		Uri ConstructUri(Uri uri, HttpQuery query)
		{
			if(query == null) {
				return uri;
			}
			StringBuilder sb = new StringBuilder();
			sb.Append(uri.AbsoluteUri);
			sb.Append(Constant.QuestionMark);
			if(uri.Query.Length > 0) {
				sb.Append(uri.Query.Substring(1));
				sb.Append(Constant.Ampersand);
			}
			sb.Append(query.ToString());
			return new Uri(sb.ToString());
		}

		string ConstructHeader()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Method.ToString().ToUpper());
			sb.Append(Constant.Space);
			if(Settings.Proxy != null) {
				sb.Append(Uri.Scheme + Uri.SchemeDelimiter + Uri.Authority);
			}
			sb.Append(Uri.PathAndQuery);
			sb.Append(Constant.Space);
			sb.Append("HTTP/" + Version);
			sb.Append(Constant.CRLF);
			sb.Append(Headers.ToString());
			sb.Append(Constant.CRLF);
			sb.Append(Constant.CRLF);
			return sb.ToString();
		}
	}
}
