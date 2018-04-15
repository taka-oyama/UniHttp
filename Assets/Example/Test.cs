using UnityEngine;
using System;
using UniHttp;
using System.Collections;
using System.Text;
using System.IO;

public class Test : MonoBehaviour
{
	HttpManager httpManager;
	HttpRequest request;

	void Awake()
	{
		var httpSettings = new HttpSettings();
		httpSettings.allowResponseCompression = false;
//		httpSettings.fileHandler = new CryptoFileHandler("testedav", "password");
		httpSettings.useCache = false;
		httpSettings.tcpNoDelay = true;
//		httpSettings.useCookies = false;
//		httpSettings.proxy = new HttpProxy("localhost", 3128);
		httpManager = HttpManager.Initalize(httpSettings);
	}

	async void Start()
	{
		var uri0 = new Uri("http://localhost:3000/random");
		// var uri0 = new Uri("http://localhost:3000/static/100mb.bin");
		this.request = new HttpRequest(HttpMethod.GET, uri0);

		httpManager.SendAsync(request);
		httpManager.SendAsync(request);
	}
}
