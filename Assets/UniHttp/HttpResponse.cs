using UnityEngine;
using System.Text;

namespace UniHttp
{
	public class HttpResponse
	{
		public HttpRequest Request { get; private set; }
		public string HttpVersion { get; internal set; }
		public int StatusCode { get; internal set; }
		public string StatusPhrase { get; internal set; }
		public HttpResponseHeaders Headers { get; private set; }
		public byte[] MessageBody { get; internal set; }

		public HttpResponse(HttpRequest request) {
			this.Request = request;
			this.Headers = new HttpResponseHeaders();
		}

		public string ToString(bool includeBody = true)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Headers.ToString());
			sb.Append(HttpRequest.CRLF);
			if(includeBody && IsStringableContentType()) {
				sb.Append(Encoding.UTF8.GetString(MessageBody));
				sb.Append(HttpRequest.CRLF);
			}
			return sb.ToString();
		}

		bool IsStringableContentType()
		{
			Debug.Log(Headers["Content-Type"][0]);
			if(Headers.Exist("Content-Type")) return false;
			if(Headers["Content-Type"][0].Contains("text/")) return true;
			if(Headers["Content-Type"][0].Contains("application/json")) return true;
			if(Headers["Content-Type"][0].Contains("application/xml")) return true;
			return false;
		}
	}
}