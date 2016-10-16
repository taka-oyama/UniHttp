using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class DebugHelper
{
	[MenuItem("Debug/Open Temporary Cache Path")]
	static void OpenTemporaryCachePath()
	{
		Process.Start(Application.temporaryCachePath);
	}
}
