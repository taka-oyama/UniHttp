using UnityEngine;
using System;

internal sealed class KeepAlive
{
	internal DateTime expiresAt;
	internal int currentCount;
	internal int maxCount;

	internal KeepAlive(DateTime expiresAt, int maxCount = int.MaxValue)
	{
		this.expiresAt = expiresAt;
		this.currentCount = 0;
		this.maxCount = maxCount;
	}

	internal bool Expired
	{
		get { return currentCount >= maxCount || DateTime.Now >= expiresAt; }
	}
}
