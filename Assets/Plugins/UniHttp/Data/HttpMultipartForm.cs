using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;

namespace UniHttp
{
	public sealed class HttpMultipartForm : IHttpData
	{
		public class Parameter
		{
			public string Boundary;
			public string Name;
			public byte[] Value;
			public string ContentType;

			public Parameter(string boundary, string name, byte[] value, string contentType)
			{
				this.Boundary = boundary;
				this.Name = name;
				this.Value = value;
				this.ContentType = contentType;
			}

			public byte[] ToBytes()
			{
				List<byte> list = new List<byte>();
				list.AddRange(Encoding.UTF8.GetBytes(ConstructHeader()));
				list.AddRange(Value);
				list.AddRange(Encoding.UTF8.GetBytes(ConstructFooter()));
				return list.ToArray();
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(ConstructHeader());
				sb.Append(ContentType == null ? Encoding.UTF8.GetString(Value) : "<Binary Data>");
				sb.Append(ConstructFooter());
				return sb.ToString();
			}

			string ConstructHeader()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Constant.Hyphen);
				sb.Append(Constant.Hyphen);
				sb.Append(Boundary);
				sb.Append(Constant.CRLF);
				sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"", Name);
				sb.Append(Constant.CRLF);
				if(ContentType != null) {
					sb.Append(HeaderField.ContentType);
					sb.Append(": ");
					sb.Append(ContentType);
					sb.Append(Constant.CRLF);
				}
				sb.Append(Constant.CRLF);
				return sb.ToString();
			}

			string ConstructFooter()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Constant.CRLF);
				sb.Append(Boundary);
				sb.Append(Constant.Hyphen);
				sb.Append(Constant.Hyphen);
				sb.Append(Constant.CRLF);
				return sb.ToString();
			}
		}

		readonly string boundary;
		readonly List<Parameter> data;

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
			return ContentType.FormData;
		}

		public void Add(string name, byte[] value, string contentType = ContentType.OctetStream)
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
