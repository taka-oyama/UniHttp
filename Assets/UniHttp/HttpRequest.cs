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

		public void Send()
		{
			ExecuteOnThread(ConnectionFlow);
		}

		void ConnectionFlow()
		{
			TcpClient socket = new TcpClient();
			socket.Connect(Uri.Host, Uri.Port);
			Stream networkStream = socket.GetStream();

			if(Uri.Scheme == Uri.UriSchemeHttps) {
				sslClient = new SslClient(Uri, networkStream, true);
				networkStream = sslClient.Authenticate(SslClient.NoVerify);
			}

			byte[] data = new RequestBuilder(this).Build();
			Debug.Log(ToString());

			networkStream.Write(data, 0, data.Length);
			networkStream.Flush();

			HttpResponse response = new ResponseBuilder(this, networkStream).Build();
			Debug.Log(response.ToString());

			if(OnComplete != null) OnComplete(response);
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
