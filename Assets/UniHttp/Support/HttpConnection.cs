using UnityEngine;
using System.Collections;
using System;
using UniRx;
using System.Net.Sockets;
using System.IO;

namespace UniHttp
{
	public class HttpConnection : IDisposable
	{
		HttpRequest request;
		CompositeDisposable disposables;

		internal HttpConnection(HttpRequest request)
		{
			this.request = request;
			this.disposables = new CompositeDisposable();
		}

		internal HttpConnection Send(Action<HttpResponse> onComplete)
		{
			Debug.Log(request.ToString());

			Scheduler.ThreadPool.Schedule(() => {
				try  {
					HttpResponse response = Transmit();
					Debug.Log(response.ToString());
					Scheduler.MainThread.Schedule(() => onComplete(response));
				}
				catch(Exception e) {
					Dispose();
					Scheduler.MainThread.Schedule(() => { throw e; });
				}
			});

			return this;
		}

		HttpResponse Transmit()
		{
			byte[] data = new RequestDataBuilder(request).Build();
			Stream networkStream = SetupStream();
			networkStream.Write(data, 0, data.Length);
			networkStream.Flush();
			return new ResponseBuilder(request, networkStream).Build();
		}

		Stream SetupStream()
		{
			TcpClient socket = new TcpClient();
			socket.Connect(request.Uri.Host, request.Uri.Port);
			Stream stream = socket.GetStream();

			if(request.Uri.Scheme == Uri.UriSchemeHttps) {
				SslClient sslClient = new SslClient(request.Uri, stream, true);
				disposables.Add(sslClient);
				return sslClient.Authenticate(SslClient.NoVerify);
			} else {
				return stream;
			}
		}

		public void Dispose()
		{
			disposables.Dispose();
		}
	}
}
