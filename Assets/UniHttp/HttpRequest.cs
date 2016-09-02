﻿using UnityEngine;
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
		public HttpRequestHeaders Headers;

		// Header Options
		public bool KeepAlive = true;

		public Action<string> OnComplete;

		public const string SPACE = " ";
		public const string CRLF = "\r\n";

		public HttpRequest(Uri uri, Methods method)
		{
			this.Uri = uri;
			this.Method = method;
			this.Headers = new HttpRequestHeaders();
		}

		public void Send()
		{
			TcpClient socket = new TcpClient();
			NetworkStream stream = null;
			socket.Connect(Uri.Host, Uri.Port);
			stream = socket.GetStream();

			var data = GenerateData();
			stream.Write(data, 0, data.Length);
			stream.Flush();

			ExecuteOnThread(() => {
				new HttpResponseParser(this, stream, 1024).Parse(res => {
					Debug.Log(res.ToString());
				});
			});
		}

		void AddDefaultHeaders()
		{
			AddHeaderIfNotExist("Host", GenerateHost());
			AddHeaderIfNotExist("User-Agent", GenerateUserAgent());

			// https://tools.ietf.org/html/rfc7230#section-6.3
			// In HTTP 1.1, all connections are considered persistent unless declared otherwise
			if(!KeepAlive) Headers.AddOrReplace("Connection", "close");
		}

		void AddHeaderIfNotExist(string key, string value)
		{
			if(!Headers.Exist(key)) Headers.Add(key, value);
		}

		byte[] GenerateData()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(GenerateHeaderString());
			sb.Append(CRLF);
			return Encoding.UTF8.GetBytes(sb.ToString());
		}

		string GenerateHeaderString()
		{
			AddDefaultHeaders();

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
			string appInfo = Application.bundleIdentifier + "/" + Application.version;
			string osInfo = SystemInfo.operatingSystem;
			return string.Format("{0} ({1}) UniHttp/1.0", appInfo, osInfo);
		}

		void ExecuteOnThread(Action action)
		{
			Scheduler.ThreadPool.Schedule(() => {
				try 
				{
					action();
				}
				catch(Exception e)
				{
					Scheduler.MainThread.Schedule(() => { throw e; });
				}
			});
		}

		public void Dispose()
		{
			
		}
	}
}
