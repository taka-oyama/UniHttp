using UnityEngine;
using System;

internal sealed class KeepAlive
{
	internal DateTime expiresAt;
	internal int currentCount;
	internal int maxCount;

	internal void Reset(DateTime newExpiresAt, int newMaxCount = 0)
	{
		this.expiresAt = newExpiresAt;
		this.currentCount = 0;
		this.maxCount = newMaxCount;
	}

	internal bool Expired
	{
		get
		{
			if(maxCount != 0 && currentCount >= maxCount) {
				return true;
			}

			if(DateTime.Now >= expiresAt) {
				return true;
			}

			return false;
		}
	}
}
