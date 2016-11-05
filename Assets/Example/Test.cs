using UnityEngine;
using System;
using UniHttp;
using System.Collections.Generic;

public class Test : MonoBehaviour {
	void Start () {
		HttpManager.Initalize();

		HttpClient client = new HttpClient();

		var uri = new Uri("http://localhost:3000/test/debug.json");
		var payload = new HttpJsonData(new TestClass() { level = 10 });

		var request = new HttpRequest(HttpMethod.GET, uri, null, payload);
		client.Send(request, response => {});
//
//		var request2 = new HttpRequest(uri, HttpMethod.GET, null, payload);
//		Debug.Log(request2);
//		client.Send(request2, response2 => {
//			Debug.Log(response2.ToString(true));
//		});
	}
}

public class TestClass
{
	public int level;
}
