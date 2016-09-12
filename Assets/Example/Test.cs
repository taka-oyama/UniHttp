using UnityEngine;
using System;
using UniRx;
using UniHttp;
using System.Collections.Generic;

public class Test : MonoBehaviour {
	void Start () {
		MainThreadDispatcher.Initialize();
		HttpDispatcher.Initalize();

		Scheduler.ThreadPool.Schedule(() => {
			var uri = new Uri("https://ec2-54-178-214-152.ap-northeast-1.compute.amazonaws.com/active_admin/login");
			var payload = new TestClass() { level = 10 };

			var request = new HttpRequest(uri, HttpMethod.GET, null, payload);
			Debug.Log(request);
			var response = request.Send();
			Debug.Log(response.ToString(true));

			Debug.Log(request);
			response = request.Send();
			Debug.Log(response.ToString(true));
		});
	}
}

public class TestClass
{
	public int level;
}
