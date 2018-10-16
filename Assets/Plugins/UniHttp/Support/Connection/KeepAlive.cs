using UnityEngine;
using System;

internal sealed class KeepAlive
{
	internal DateTime ExpiresAt;
	internal int CurrentCount;
	internal int MaxCount;

	internal void Reset(DateTime newExpiresAt, int newMaxCount = 0)
	{
		this.ExpiresAt = newExpiresAt;
		this.CurrentCount = 0;
		this.MaxCount = newMaxCount;
	}

	internal bool Expired
	{
		get
		{
			if(MaxCount != 0 && CurrentCount >= MaxCount) {
				return true;
			}

			if(DateTime.Now >= ExpiresAt) {
				return true;
			}

			return false;
		}
	}
}
