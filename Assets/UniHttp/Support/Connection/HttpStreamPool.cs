using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UniHttp
{
	internal sealed class HttpStreamPool
	{
		object locker;
		List<HttpStream> streams;

		internal HttpStreamPool()
		{
			this.locker = new object();
			this.streams = new List<HttpStream>();
		}

		internal HttpStream CheckOut(HttpRequest request)
		{
			lock(locker) {
				string url = string.Concat(request.Uri.Scheme, Uri.SchemeDelimiter, request.Uri.Authority);
				int index = streams.FindIndex(s => s.url == url);
				if(index >= 0) {
					var stream = streams[index];
					streams.RemoveAt(index);
					if(stream.Connected) {
						return stream;
					}
				}
				return new HttpStream(request.Uri);
			}
		}

		internal void CheckIn(HttpResponse response, HttpStream stream)
		{
			if(!IsPersistedConnection(response)) {
				stream.Close();
				return;
			}

			lock(locker) {
				streams.Add(stream);

				// clean up closed streams
				streams.RemoveAll(s => !s.Connected);
			}
		}

		bool IsPersistedConnection(HttpResponse response)
		{
			if(response.Request.Headers.Exist("Connection", "close")) {
				return false;
			}
			if(!response.Headers.Exist("Keep-Alive")) {
				return false;
			}
			return true;
		}

		internal void CloseAll()
		{
			lock(locker) {
				streams.ForEach(stream => stream.Close());
				streams.Clear();
			}
		}
	}
}
