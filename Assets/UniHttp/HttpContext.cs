using UnityEngine;

namespace UniHttp
{
	public class HttpContext : MonoBehaviour
	{
		void Awake()
		{
		}

		void OnApplicationPause(bool isPaused)
		{
			if(isPaused) {
				HttpDispatcher.Save();
			}
		}

		void OnApplicationQuit()
		{
			HttpDispatcher.Save();
		}
	}
}
