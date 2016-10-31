using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace UniHttp
{
	public sealed class HttpFormData : IHttpData
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

			public byte[] ToByteArray()
			{
				List<byte> list = new List<byte>();
				list.AddRange(Encoding.UTF8.GetBytes("--" + boundary + Constant.CRLF));
				list.AddRange(Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"" + name + "\""));
				list.AddRange(Encoding.UTF8.GetBytes(Constant.CRLF + Constant.CRLF));
				list.AddRange(value);
				list.AddRange(Encoding.UTF8.GetBytes(Constant.CRLF));
				list.AddRange(Encoding.UTF8.GetBytes(boundary + "--" + Constant.CRLF));
				return list.ToArray();
			}
		}

		string boundary;
		List<Parameter> data;

		public HttpFormData(string boundary)
		{
			this.boundary = boundary;
			this.data = new List<Parameter>();
		}

		public string GetContentType()
		{
			return "multipart/form-data";
		}

		public bool Contains(string name)
		{
			for(int i = 0; i < data.Count; i++) {
				if(data[i].name == name) {
					return true;
				}
			}
			return false;
		}

		public void Add(string name, byte[] value, string contentType = null)
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
				list.AddRange(data[i].ToByteArray());
			}
			return list.ToArray();
		}
	}
}
