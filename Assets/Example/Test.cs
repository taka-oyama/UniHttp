using UnityEngine;
using System;
using UniRx;
using UniHttp;
using System.Collections.Generic;

public class Test : MonoBehaviour {
	void Start () {
		MainThreadDispatcher.Initialize();
		HttpManager.Initalize();

		HttpClient client = new HttpClient();

		var uri = new Uri("http://www.google.com");
		var payload = new TestClass() { level = 10 };

		var request = new HttpRequest(uri, HttpMethod.GET, null, null);
		Debug.Log(request);
		client.Send(request, response => {
			Debug.Log(response.ToString(true));

			request = new HttpRequest(uri, HttpMethod.GET, null, payload);
			Debug.Log(request);
			client.Send(request, response2 => {
				Debug.Log(response2.ToString(true));
			});
		});
	}
}

public class TestClass
{
	public int level;
}
