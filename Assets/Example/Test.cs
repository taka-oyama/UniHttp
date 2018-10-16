using UnityEngine;
using System;
using UniHttp;
using System.Net;
using System.Threading.Tasks;

public class Test : MonoBehaviour
{
	HttpManager httpManager;
	HttpRequest request;

	void Awake()
	{
		var context = new HttpContext();
		context.AllowCompressedResponse = false;
//		httpSettings.FileHandler = new CryptoFileHandler("testedav", "password");
		context.UseCache = false;
		context.TcpNoDelay = true;
		context.UseCookies = false;
//		httpSettings.Proxy = new HttpProxy("localhost", 3128);
		httpManager = HttpManager.Initalize(context);

		var uri0 = new Uri("http://localhost:3000/random");
		// var uri0 = new Uri("http://localhost:3000/static/100mb.bin");
		this.request = new HttpRequest(HttpMethod.GET, uri0);
	}

	async void Update()
	{
		await httpManager.SendAsync(request);
	}
}
