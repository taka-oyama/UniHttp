using UnityEngine;
using System;

namespace UniHttp
{
	public class HttpProxy
	{
		internal Uri Uri { get; private set; }

		public HttpProxy(string host, int port)
		{
			this.Uri = new Uri("tcp://" + host + ":" + port);
		}
	}
}
