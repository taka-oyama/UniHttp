using UnityEngine;
using System;
using UniRx;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		UniRx.MainThreadDispatcher.Initialize();
		new UniHttp.HttpRequest(new Uri("http://checkgzipcompression.com/"), UniHttp.HttpRequest.Methods.GET).Send();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
