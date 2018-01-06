using UnityEngine;
using System.Text;

internal static class UserAgent
{
	static string value;

	internal static string Value
	{
		get 
		{
			return value = value ?? Build();
		}
	}

	internal static string Build()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(Application.identifier);
		sb.Append("/");
		sb.Append(Application.version);
		sb.Append(" (");
		sb.Append(SystemInfo.deviceModel);
		sb.Append("; ");
		sb.Append(SystemInfo.operatingSystem);
		sb.Append(")");
		return sb.ToString();
	}
}
