using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using UniRx;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;

namespace UniHttp
{
	public class HttpRequest : IDisposable
	{
		public enum Methods : byte { GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS }

		public Methods Method { get; private set; } 
		public Uri Uri { get; private set; }
		public string Version { get { return "1.1"; } }
		public RequestHeaders Headers { get; private set; }

		public bool KeepAlive = true;
		public bool Compress = true;

		public Action<HttpResponse> OnComplete;

		SslClient sslClient;

		public HttpRequest(Uri uri, Methods method)
		{
			this.Uri = uri;
			this.Method = method;
			this.Headers = new RequestHeadersDefaultBuilder(this).Build();
		}

		public HttpRequest Send()
		{
			byte[] data = new RequestDataBuilder(this).Build();
			Debug.Log(ToString());
			ExecuteOnThread(() => {
				HttpResponse response = ConnectionFlow(data);
				Debug.Log(response.ToString());

				if(OnComplete != null) {
					Scheduler.MainThread.Schedule(() => OnComplete(response));
				}
			});
			return this;
		}

		HttpResponse ConnectionFlow(byte[] data)
		{
			TcpClient socket = new TcpClient();
			socket.Connect(Uri.Host, Uri.Port);
			Stream networkStream = socket.GetStream();

			if(Uri.Scheme == Uri.UriSchemeHttps) {
				sslClient = new SslClient(Uri, networkStream, true);
				networkStream = sslClient.Authenticate(SslClient.NoVerify);
			}

			networkStream.Write(data, 0, data.Length);
			networkStream.Flush();

			return new ResponseBuilder(this, networkStream).Build();
		}

		void ExecuteOnThread(Action action)
		{
			Scheduler.ThreadPool.Schedule(() => {
				try  {
					action();
				}
				catch(Exception e) {
					Dispose();
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
			if(sslClient != null) sslClient.Dispose();
			this.OnComplete = null;
		}
	}
}
