using UnityEngine;
using System.Text;

internal static class UserAgent
{
	internal static string value;

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
		return value = sb.ToString();
	}
}
