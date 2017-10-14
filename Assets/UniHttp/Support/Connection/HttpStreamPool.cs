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
		TimeSpan keepAliveTimeout;
		ISslVerifier sslVerifier;

		internal HttpStreamPool(TimeSpan keepAliveTimeout, ISslVerifier sslVerifier)
		{
			this.locker = new object();
			this.streams = new List<HttpStream>();
			this.keepAliveTimeout = keepAliveTimeout;
			this.sslVerifier = sslVerifier;
		}

		internal HttpStream CheckOut(HttpRequest request)
		{
			string url = string.Concat(request.Uri.Scheme, Uri.SchemeDelimiter, request.Uri.Authority);
			DateTime expiresAt = DateTime.Now + keepAliveTimeout;

			lock(locker) {
				int index = streams.FindIndex(s => s.url == url);
				if(index >= 0) {
					HttpStream stream = streams[index];
					streams.RemoveAt(index);
					if(stream.Connected) {
						return stream;
					}
				}
			}
			return new HttpStream(request.Uri, expiresAt, sslVerifier);
		}

		internal void CheckIn(HttpResponse response, HttpStream stream)
		{
			if(!IsPersistedConnection(response)) {
				stream.Close();
				return;
			}

			UpdateKeepAliveInfo(response, stream);

			if(stream.keepAlive.Expired) {
				stream.Close();
				return;
			}

			lock(locker) {
				streams.Add(stream);
				streams.RemoveAll(s => !s.Connected);
			}
		}

		bool IsPersistedConnection(HttpResponse response)
		{
			if(response.Request.Headers.Exist("Connection", "close")) {
				return false;
			}
			return true;
		}

		void UpdateKeepAliveInfo(HttpResponse response, HttpStream stream)
		{
			if(response.Headers.Exist("Keep-Alive")) {
				DateTime now = response.Headers.Exist("Date") ? Helper.ParseDate(response.Headers["Date"][0]) : DateTime.Now;

				foreach(string parameter in response.Headers["Keep-Alive"][0].Split(',')) {
					string[] pair = parameter.Trim().Split('=');

					if(pair[0] == "timeout") {
						stream.keepAlive.expiresAt = now.AddSeconds(int.Parse(pair[1]));
						continue;
					}
					if(pair[0] == "max") {
						stream.keepAlive.maxCount = int.Parse(pair[1]);
						continue;
					}
				}
			}
			stream.keepAlive.currentCount += 1;
		}

		internal void CheckExpiredStreams()
		{
			if(streams.Count > 0) {
				foreach(HttpStream stream in streams) {
					if(stream.keepAlive.Expired) {
						stream.Close();
					}
				}
				lock(locker) {
					streams.RemoveAll(s => !s.Connected);
				}
			}
		}

		internal void CloseAll()
		{
			lock(locker) {
				foreach(HttpStream stream in streams) {
					stream.Close();
				}
				streams.Clear();
			}
		}
	}
}
