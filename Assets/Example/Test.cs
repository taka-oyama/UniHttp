using UnityEngine;
using System;
using UniRx;
using UniHttp;
using System.Collections.Generic;

public class Test : MonoBehaviour {
	void Start () {
		MainThreadDispatcher.Initialize();
		HttpDispatcher.Initalize();

		HttpClient client = new HttpClient();

		Scheduler.ThreadPool.Schedule(() => {
			try {
				var uri = new Uri("http://ec2-54-178-214-152.ap-northeast-1.compute.amazonaws.com/active_admin/login");
				var payload = new TestClass() { level = 10 };

				var request = new HttpRequest(uri, HttpMethod.GET, null, null);
				Debug.Log(request);
				var response = client.Send(request);
				Debug.Log(response.ToString(true));

				request = new HttpRequest(uri, HttpMethod.GET, null, payload);
				Debug.Log(request);
				response = client.Send(request);
				Debug.Log(response.ToString(true));
			}
			catch(Exception e) {
				Scheduler.MainThread.Schedule(() => { throw e; });
			}
		});
	}
}

public class TestClass
{
	public int level;
}
