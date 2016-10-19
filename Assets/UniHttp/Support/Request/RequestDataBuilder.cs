using UnityEngine;
using System.Text;

namespace UniHttp
{	
	public class RequestDataBuilder
	{
		internal const string SPACE = " ";
		internal const string CRLF = "\r\n";

		HttpRequest request;

		public RequestDataBuilder(HttpRequest request)
		{
			this.request = request;
		}

		public byte[] Build()
		{
			StringBuilder sb = new StringBuilder();
			AppendHeaderString(sb);
			AppendPayloadString(sb);
			return Encoding.UTF8.GetBytes(sb.ToString());
		}

		void AppendHeaderString(StringBuilder sb)
		{
			sb.Append(request.Method.ToString().ToUpper());
			sb.Append(SPACE);
			sb.Append(request.Uri.PathAndQuery);
			sb.Append(SPACE);
			sb.Append("HTTP/" + request.Version);
			sb.Append(CRLF);
			sb.Append(request.Headers.ToString());
			sb.Append(CRLF);
			sb.Append(CRLF);
		}

		void AppendPayloadString(StringBuilder sb)
		{
			if(request.Payload != null) {
				sb.Append(HttpManager.JsonSerializer.Serialize(request.Payload));
				sb.Append(CRLF);
			}
		}
	}
}
