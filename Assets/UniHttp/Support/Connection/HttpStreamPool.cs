using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading;

namespace UniHttp
{
	internal sealed class HttpStreamPool
	{
		object locker;
		List<HttpStream> streams;

		internal HttpStreamPool(int maxCount)
		{
			this.locker = new object();
			this.streams = new List<HttpStream>(maxCount);
		}

		internal HttpStream CheckOut(HttpRequest request)
		{
			lock(locker) {
				return Generate(request.Uri);
			}
		}

		internal void CheckIn(HttpResponse response, HttpStream stream)
		{
			if(IsPersistentResponse(response)) {
				
			}
			stream.Close();
		}

		bool IsPersistentResponse(HttpResponse response)
		{
			if(response.Request.Headers.Exist("Connection", "close")) {
				return false;
			}
			if(!response.Headers.Exist("Keep-Alive")) {
				return false;
			}
			return true;
		}

		HttpStream Generate(Uri uri)
		{
			return new HttpStream(uri);
		}

		internal void Clear()
		{
			lock(locker) {
				streams.ForEach(stream => stream.Close());
				streams.Clear();
			}
		}
	}
}
