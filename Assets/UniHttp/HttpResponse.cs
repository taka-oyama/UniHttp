using UnityEngine;
using System.Text;
using System;

namespace UniHttp
{
	public class HttpResponse
	{
		public HttpRequest Request { get; private set; }
		public string HttpVersion { get; internal set; }
		public int StatusCode { get; internal set; }
		public string StatusPhrase { get; internal set; }
		public ResponseHeaders Headers { get; private set; }
		public byte[] MessageBody { get; internal set; }
		public TimeSpan RoundTripTime { get; internal set; }

		public HttpResponse(HttpRequest request) {
			this.Request = request;
			this.Headers = new ResponseHeaders();
		}

		public override string ToString()
		{
			return ToString(IsStringableContentType());
		}

		public string ToString(bool showMessageBody = false)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(HttpVersion);
			sb.Append(" ");
			sb.Append(StatusCode);
			sb.Append(" ");
			sb.Append(StatusPhrase);
			sb.Append("\n");
			if(Headers.Length > 0) {
				sb.Append(Headers.ToString());
				sb.Append("\n");
			}
			if(showMessageBody) {
				sb.Append("\n");
				sb.Append(Encoding.UTF8.GetString(MessageBody));
				sb.Append("\n");
			}
			return sb.ToString();
		}

		bool IsStringableContentType()
		{
			if(Headers.NotExist("Content-Type")) return false;
			if(Headers["Content-Type"][0].Contains("text/")) return true;
			if(Headers["Content-Type"][0].Contains("application/json")) return true;
			if(Headers["Content-Type"][0].Contains("application/xml")) return true;
			return false;
		}
	}
}