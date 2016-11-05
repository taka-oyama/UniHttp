using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System;

namespace UniHttp
{
	public sealed class HttpMultipartForm : IHttpData
	{
		public class Parameter
		{
			public string boundary;
			public string name;
			public byte[] value;
			public string contentType;

			public Parameter(string boundary, string name, byte[] value, string contentType)
			{
				this.boundary = boundary;
				this.name = name;
				this.value = value;
				this.contentType = contentType;
			}

			public byte[] ToBytes()
			{
				List<byte> list = new List<byte>();
				list.AddRange(Encoding.UTF8.GetBytes(ConstructHeader()));
				list.AddRange(value);
				list.AddRange(Encoding.UTF8.GetBytes(ConstructFooter()));
				return list.ToArray();
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(ConstructHeader());
				sb.Append(contentType == null ? Encoding.UTF8.GetString(value) : "<Binary Data>");
				sb.Append(ConstructFooter());
				return sb.ToString();
			}

			string ConstructHeader()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("--");
				sb.Append(boundary);
				sb.Append(Constant.CRLF);
				sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"", name);
				sb.Append(Constant.CRLF);
				if(contentType != null) {
					sb.Append("Content-Type: ");
					sb.Append(contentType);
					sb.Append(Constant.CRLF);
				}
				sb.Append(Constant.CRLF);
				return sb.ToString();
			}

			string ConstructFooter()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Constant.CRLF);
				sb.Append(boundary);
				sb.Append("--");
				sb.Append(Constant.CRLF);
				return sb.ToString();
			}
		}

		string boundary;
		List<Parameter> data;

		public HttpMultipartForm(string boundary)
		{
			this.boundary = boundary;
			this.data = new List<Parameter>();
		}

		public HttpMultipartForm() : this("----FormBoundary" + Guid.NewGuid().ToString("N").Substring(0, 8))
		{
		}

		public string GetContentType()
		{
			return "multipart/form-data";
		}

		public void Add(string name, byte[] value, string contentType = "application/octet-stream")
		{
			data.Add(new Parameter(boundary, name, value, contentType));
		}

		public void Add(string name, string value, string contentType = null)
		{
			data.Add(new Parameter(boundary, name, Encoding.UTF8.GetBytes(value), contentType));
		}

		public byte[] ToBytes()
		{
			List<byte> list = new List<byte>();
			for(int i = 0; i < data.Count; i++) {
				list.AddRange(data[i].ToBytes());
			}
			return list.ToArray();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < data.Count; i++) {
				sb.Append(data[i].ToString());
			}
			return sb.ToString();
		}
	}
}
