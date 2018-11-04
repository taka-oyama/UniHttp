﻿using System.Text;
using System;
using System.Collections.Generic;

namespace UniHttp
{
	public sealed class HttpResponse
	{
		public HttpRequest Request { get; }
		public string HttpVersion { get; internal set; }
		public int StatusCode { get; internal set; }
		public string StatusPhrase { get; internal set; }
		public ResponseHeaders Headers { get; }
		public List<Cookie> Cookies { get; internal set; }
		public byte[] MessageBody { get; internal set; }
		public TimeSpan Duration { get; internal set; }

		public HttpResponse(HttpRequest request)
		{
			this.Request = request;
			this.Headers = new ResponseHeaders();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(HttpVersion);
			sb.Append(Constant.Space);
			sb.Append(StatusCode);
			sb.Append(Constant.Space);
			sb.Append(StatusPhrase);
			sb.Append(Constant.Space);
			sb.AppendFormat(" ({0} ms)", Duration.TotalMilliseconds);
			sb.Append(Constant.CRLF);
			if(Headers.Length > 0) {
				sb.Append(Headers.ToString());
				sb.Append(Constant.CRLF);
			}
			if(MessageBody.Length > 0) {
				sb.Append(Constant.CRLF);
				sb.Append(MessageBodyAsString());
				sb.Append(Constant.CRLF);
			}
			return sb.ToString();
		}

		bool IsStringableContentType()
		{
			if(Headers.Contains(HeaderField.ContentType)) {
				if(Headers[HeaderField.ContentType][0].Contains("text/")) return true;
				if(Headers[HeaderField.ContentType][0].Contains(ContentType.Json)) return true;
				if(Headers[HeaderField.ContentType][0].Contains(ContentType.Xml)) return true;
			}
			return false;
		}

		string MessageBodyAsString()
		{
			int maxSize = 1024 * 1024;
			int bufferSize = Math.Min(MessageBody.Length, maxSize);
			StringBuilder sb = new StringBuilder();

			if(IsStringableContentType()) {
				byte[] buffer = new byte[bufferSize];
				Buffer.BlockCopy(MessageBody, 0, buffer, 0, bufferSize);
				sb.Append(Encoding.UTF8.GetString(buffer));
				if(buffer.Length == maxSize) {
					sb.Append("...<too much data to print>");
				}
			} else {
				sb.AppendFormat("<Binary Data (Size: {0})>", MessageBody.Length.ToString());
			}
			return sb.ToString();
		}
	}
}