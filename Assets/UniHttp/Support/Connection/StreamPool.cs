using UnityEngine;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace UniHttp
{
	internal sealed class StreamPool
	{
		readonly object locker;
		readonly HttpSettings settings;
		readonly List<HttpStream> unusedStreams;
		readonly List<HttpStream> usedStreams;

		internal StreamPool(HttpSettings settings)
		{
			this.locker = new object();
			this.settings = settings;
			this.unusedStreams = new List<HttpStream>();
			this.usedStreams = new List<HttpStream>();
		}

		internal HttpStream CheckOut(HttpRequest request)
		{
			string baseUrl = string.Concat(request.Uri.Scheme, Uri.SchemeDelimiter, request.Uri.Authority);

			lock(locker) {
				int index = unusedStreams.FindIndex(s => s.baseUrl == baseUrl);
				if(index >= 0) {
					HttpStream stream = unusedStreams[index];
					unusedStreams.RemoveAt(index);
					if(stream.Connected) {
						return stream;
					}
				}

				Uri uri = request.useProxy ? settings.proxy.Uri : request.Uri;
				HttpStream newStream = new HttpStream(uri, settings);
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
			if(response == null) {
				return false;
			}

			if(response.Request.Headers.Exist(HeaderField.Connection, "close")) {
				return false;
			}
			return true;
		}

		void UpdateKeepAliveInfo(HttpResponse response, HttpStream stream)
		{
			if(response.Headers.Exist(HeaderField.KeepAlive)) {
				foreach(string parameter in response.Headers[HeaderField.KeepAlive][0].Split(',')) {
					string[] pair = parameter.Trim().Split('=');

					if(pair[0] == "timeout") {
						DateTime now;
						if(response.Headers.Exist(HeaderField.Date)) {
							now = DateTime.ParseExact(
								response.Headers[HeaderField.Date][0],
								CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern,
								CultureInfo.InvariantCulture
							);
						} else {
							now = DateTime.Now;
						}
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
