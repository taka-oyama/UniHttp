using UnityEngine;

namespace UniHttp
{
	public class HttpManager : MonoBehaviour
	{
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
