using UnityEngine;
using System;
using UniHttp;
using System.Collections;

public class Test : MonoBehaviour
{
	HttpManager httpManager;

	void Awake()
	{
		var httpSettings = new HttpSettings();
//		httpSettings.useCache = false;
//		httpSettings.proxy = new HttpProxy("localhost", 3128);
		httpManager = HttpManager.Initalize(httpSettings);
	}

	public void Send ()
	{
		var uri0 = new Uri("http://localhost:3000/large_file");
		var request0 = new HttpRequest(HttpMethod.GET, uri0);

		httpManager.Send(request0, response => {
		});

		httpManager.Send(request0, response => {
		});
	}
}
