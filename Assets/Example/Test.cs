using UnityEngine;
using System;
using UniHttp;

public class Test : MonoBehaviour
{
	void Start ()
	{
		HttpManager.Initalize();
		HttpClient client = new HttpClient();

		var uri = new Uri("http://localhost:3000/test/debug.json");
		object payload = new HttpJsonData(new TestClass() { level = 10, stat = "!!#^(0-=" });
		var request = new HttpRequest(HttpMethod.GET, uri);
		client.Send(request, response => {});

		var uri1 = new Uri("http://rubyonrails.org/");
		var request1 = new HttpRequest(HttpMethod.GET, uri1);
		client.Send(request1, response => {});
	}
}

public class TestClass
{
	public int level;
	public string stat;
}
