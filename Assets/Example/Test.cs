using UnityEngine;
using System;
using UniRx;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		UniRx.MainThreadDispatcher.Initialize();
		new UniHttp.HttpRequest(new Uri("https://54.178.214.152/active_admin/login"), UniHttp.HttpRequest.Methods.GET).Send();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
