using UnityEngine;
using System;
using UniRx;
using UniHttp;

public class Test : MonoBehaviour {
	void Start () {
		UniRx.MainThreadDispatcher.Initialize();
		var request = new HttpRequest(new Uri("https://localhost:3000/active_admin/login"), HttpMethod.GET);
		Debug.Log(request);
		var response = request.Send();
		Debug.Log(response.ToString(true));
	}
}
