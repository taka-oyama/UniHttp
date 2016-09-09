using UnityEngine;
using System;
using UniRx;
using UniHttp;

public class Test : MonoBehaviour {
	void Start () {
		MainThreadDispatcher.Initialize();

		Scheduler.ThreadPool.Schedule(() => {
			var request = new HttpRequest(new Uri("https://54.178.214.152/active_admin/login"), HttpMethod.GET);
			Debug.Log(request);
			var response = request.Send();
			Debug.Log(response.ToString(true));
		});
	}
}
