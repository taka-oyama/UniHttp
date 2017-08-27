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

		internal HttpStreamPool(int maxConnectionCount)
		{
			this.locker = new object();
			this.streams = new List<HttpStream>(maxConnectionCount);

			if(streams.Capacity < 1) {
				throw new OverflowException("maxCount must be greater than 0");
			}
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
				if(streams.Capacity == streams.Count) {
					if(streams[0].Connected) {
						streams[0].Close();
					}
					streams.RemoveAt(0);
				}
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
