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
			sb.Append(GenerateHeaderString());
			sb.Append(CRLF);
			return Encoding.UTF8.GetBytes(sb.ToString());
		}

		string GenerateHeaderString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(request.Method.ToString().ToUpper());
			sb.Append(SPACE);
			sb.Append(request.Uri.PathAndQuery);
			sb.Append(SPACE);
			sb.Append("HTTP/" + request.Version);
			sb.Append(CRLF);
			sb.Append(request.Headers.ToString());
			sb.Append(CRLF);
			if(request.Payload != null) {
				sb.Append(CRLF);
				sb.Append(HttpDispatcher.JsonSerializer.Serialize(request.Payload));
				sb.Append(CRLF);
			}

			return sb.ToString();
		}
	}
}
