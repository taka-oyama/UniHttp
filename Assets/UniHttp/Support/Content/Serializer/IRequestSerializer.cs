using UnityEngine;

namespace UniHttp
{
	public interface IContentSerializer
	{
		string Serialize<T>(T target);
	}
}
