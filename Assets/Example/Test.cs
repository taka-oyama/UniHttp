using UnityEngine;
using System;
using UniHttp;
using System.Collections;

public class Test : MonoBehaviour
{
	HttpManager httpManager;

	void Awake()
	{
		httpManager = HttpManager.Initalize();
	}

	void Start ()
	{
		var uri0 = new Uri("http://localhost:3000/large_file");
		var request0 = new HttpRequest(HttpMethod.GET, uri0);
		httpManager.Send(request0, response => {
		});

		var uri = new Uri("http://localhost:3000/test");
		IHttpData payload = new HttpJsonData(new TestClass() { level = 10, stat = "!!#^(0-=" });
		var request = new HttpRequest(HttpMethod.GET, uri);
		httpManager.Send(request, response => {
			var uri1 = new Uri("http://localhost:3000/test/debug");
			var request1 = new HttpRequest(HttpMethod.GET, uri1);
			httpManager.Send(request1, r => {
			});
		});

	}
}

public class TestClass
{
	public int level;
	public string stat;
}
