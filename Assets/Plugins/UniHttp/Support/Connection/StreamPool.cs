using UnityEngine;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace UniHttp
{
	internal sealed class StreamPool
	{
		readonly object locker;
		readonly ISslVerifier sslVerifier;
		readonly List<HttpStream> unusedStreams;
		readonly List<HttpStream> usedStreams;

		internal StreamPool(ISslVerifier sslVerifier)
		{
			this.locker = new object();
			this.sslVerifier = sslVerifier;
			this.unusedStreams = new List<HttpStream>();
			this.usedStreams = new List<HttpStream>();
		}

		internal async Task<HttpStream> CheckOutAsync(HttpRequest request)
		{
			Uri uri = request.Settings.Proxy?.Uri ?? request.Uri;
			string baseUrl = uri.Scheme + Uri.SchemeDelimiter + uri.Authority;

			HttpStream stream = null;

			lock(locker) {
				int index = unusedStreams.FindIndex(s => s.BaseUrl == baseUrl);

				if(index >= 0) {
					HttpStream unusedStream = unusedStreams[index];
					unusedStreams.RemoveAt(index);
					if(unusedStream.Connected) {
						stream = unusedStream;
					}
				}

				stream = stream ?? new HttpStream(uri, sslVerifier);
				stream.UpdateSettings(request.Settings);

				usedStreams.Add(stream);
			}

			if(!stream.Connected) {
				await stream.ConnectAsync();
			}

			return stream;
		}

		internal void CheckIn(HttpResponse response, HttpStream stream)
		{
			if(stream == null) {
				return;
			}

			lock(locker) {
				usedStreams.Remove(stream);

				if(!IsPersistedConnection(response)) {
					stream.Close();
					return;
				}

				UpdateKeepAliveInfo(response, stream);

				if(stream.KeepAlive.Expired) {
					stream.Close();
					return;
				}

				unusedStreams.Add(stream);
			}
		}

		bool IsPersistedConnection(HttpResponse response)
		{
			if(response == null) {
				return false;
			}

			if(response.Request.Headers.Contains(HeaderField.Connection, HeaderValue.Close)) {
				return false;
			}
			return true;
		}

		void UpdateKeepAliveInfo(HttpResponse response, HttpStream stream)
		{
			if(response.Headers.Contains(HeaderField.KeepAlive)) {
				foreach(string parameter in response.Headers[HeaderField.KeepAlive][0].Split(',')) {
					string[] pair = parameter.Trim().Split('=');
					if(pair[0] == HeaderValue.Timeout) {
						DateTime now;
						if(response.Headers.Contains(HeaderField.Date)) {
							now = DateTime.ParseExact(
								response.Headers[HeaderField.Date][0],
								CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern,
								CultureInfo.InvariantCulture
							);
						} else {
							now = DateTime.Now;
						}
						stream.KeepAlive.ExpiresAt = now.AddSeconds(int.Parse(pair[1]));
						continue;
					}
					if(pair[0] == HeaderValue.Max) {
						stream.KeepAlive.MaxCount = int.Parse(pair[1]);
						continue;
					}
				}
			}
			stream.KeepAlive.CurrentCount += 1;
		}

		internal void CheckExpiredStreams()
		{
			if(unusedStreams.Count > 0) {
				foreach(HttpStream stream in unusedStreams) {
					if(stream.KeepAlive.Expired) {
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
