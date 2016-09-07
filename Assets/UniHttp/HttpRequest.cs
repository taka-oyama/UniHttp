using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using UniRx;

namespace UniHttp
{
	public class HttpRequest : IDisposable
	{
		public enum Methods : byte { GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS }

		public Methods Method { get; private set; } 
		public Uri Uri { get; private set; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; private set; }

		string appInfo = Application.bundleIdentifier + "/" + Application.version;
		string osInfo = SystemInfo.operatingSystem;

		// Header Options
		public bool KeepAlive = true;

		public Action<HttpResponse> OnComplete;

		public const string SPACE = " ";
		public const string CRLF = "\r\n";

		public HttpRequest(Uri uri, Methods method)
		{
			this.Uri = uri;
			this.Method = method;
			this.Headers = new RequestHeaders();
			Headers.Add("Host", GenerateHost());
			Headers.Add("User-Agent", GenerateUserAgent());
			Headers.Add("Accept-Encoding", "gzip");
		}

		public void Send()
		{
			TcpClient socket = new TcpClient();
			NetworkStream stream = null;
			socket.Connect(Uri.Host, Uri.Port);
			stream = socket.GetStream();

			ExecuteOnThread(() => {
				var data = MessageToBytes();
				Debug.Log(ToString());
				stream.Write(data, 0, data.Length);
				stream.Flush();
				var response = new ResponseBuilder(this, stream, socket.ReceiveBufferSize).Parse();
				Debug.Log(response.ToString());
				if(OnComplete != null) OnComplete(response);
			});
		}

		byte[] MessageToBytes()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(GenerateHeaderString());
			sb.Append(CRLF);
			return Encoding.UTF8.GetBytes(sb.ToString());
		}

		string GenerateHeaderString()
		{
			// https://tools.ietf.org/html/rfc7230#section-6.3
			// In HTTP 1.1, all connections are considered persistent unless declared otherwise
			if(!KeepAlive) Headers.AddOrReplace("Connection", "close");

			StringBuilder sb = new StringBuilder();
			sb.Append(Method.ToString().ToUpper());
			sb.Append(SPACE);
			sb.Append(Uri.PathAndQuery);
			sb.Append(SPACE);
			sb.Append("HTTP/" + Version);
			sb.Append(CRLF);
			sb.Append(Headers.ToString());
			sb.Append(CRLF);

			return sb.ToString();
		}

		string GenerateHost()
		{
			string host = Uri.Host;
			if(Uri.Scheme == Uri.UriSchemeHttp && Uri.Port != 80 ||
			   Uri.Scheme == Uri.UriSchemeHttps && Uri.Port != 443)
			{
				host += ":" + Uri.Port; 
			}
			return host;
		}

		string GenerateUserAgent()
		{
			return string.Format("{0} ({1}) UniHttp/1.0", appInfo, osInfo);
		}

		void ExecuteOnThread(Action action)
		{
			Scheduler.ThreadPool.Schedule(() => {
				try  {
					action();
				}
				catch(Exception e) {
					Scheduler.MainThread.Schedule(() => { throw e; });
				}
			});
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Method.ToString().ToUpper());
			sb.Append(" ");
			sb.Append(Uri.ToString());
			sb.Append("\n");
			sb.Append(Headers.ToString());
			sb.Append("\n");
			return sb.ToString();
		}

		public void Dispose()
		{
			this.OnComplete = null;
		}
	}
}
