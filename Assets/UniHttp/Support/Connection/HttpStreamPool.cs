using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UniHttp
{
	internal sealed class HttpStreamPool
	{
		object locker;
		List<HttpStream> unusedStreams;
		List<HttpStream> usedStreams;
		TimeSpan keepAliveTimeout;
		ISslVerifier sslVerifier;
		bool tcpNoDelay;
		int tcpBufferSize;

		internal HttpStreamPool(HttpSettings settings)
		{
			this.locker = new object();
			this.unusedStreams = new List<HttpStream>();
			this.usedStreams = new List<HttpStream>();
			this.keepAliveTimeout = settings.keepAliveTimeout;
			this.sslVerifier = settings.sslVerifier;
			this.tcpNoDelay = settings.tcpNoDelay;
			this.tcpBufferSize = settings.tcpBufferSize;
		}

		internal HttpStream CheckOut(HttpRequest request)
		{
			string baseUrl = string.Concat(request.Uri.Scheme, Uri.SchemeDelimiter, request.Uri.Authority);
			DateTime expiresAt = DateTime.Now + keepAliveTimeout;

			lock(locker) {
				int index = unusedStreams.FindIndex(s => s.baseUrl == baseUrl);
				if(index >= 0) {
					HttpStream stream = unusedStreams[index];
					unusedStreams.RemoveAt(index);
					if(stream.Connected) {
						return stream;
					}
				}

				HttpStream newStream = new HttpStream(request.Uri, expiresAt, sslVerifier);
				newStream.TcpClient.SendBufferSize = tcpBufferSize;
				newStream.TcpClient.ReceiveBufferSize = tcpBufferSize;
				newStream.TcpClient.NoDelay = tcpNoDelay;
				newStream.Connect();
				usedStreams.Add(newStream);

				return newStream;
			}
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
				usedStreams.Remove(stream);
				unusedStreams.Add(stream);
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
			if(unusedStreams.Count > 0) {
				foreach(HttpStream stream in unusedStreams) {
					if(stream.keepAlive.Expired) {
						stream.Close();
					}
				}
				lock(locker) {
					unusedStreams.RemoveAll(s => !s.Connected);
				}
			}
		}

		internal void CloseAll()
		{
			lock(locker) {
				foreach(HttpStream stream in unusedStreams) {
					stream.Close();
				}
				foreach(HttpStream stream in usedStreams) {
					stream.Close();
				}
				unusedStreams.Clear();
				usedStreams.Clear();
			}
		}
	}
}
