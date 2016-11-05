using UnityEngine;
using System;
using UniHttp;

public class Test : MonoBehaviour {
	void Start () {
		HttpManager.Initalize();

		HttpClient client = new HttpClient();
		var uri = new Uri("http://localhost:3000/test/debug.json");
		var payload = new HttpJsonData(new TestClass() { level = 10, stat = "!!#^(0-=" });
		var request = new HttpRequest(HttpMethod.GET, uri, payload);
		client.Send(request, response => {});
	}
}

public class TestClass
{
	public int level;
	public string stat;
}
