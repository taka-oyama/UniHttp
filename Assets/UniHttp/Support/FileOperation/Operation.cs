using UnityEngine;
using System;

namespace UniHttp.FileOperation
{
	internal abstract class Operation : IDisposable
	{
		internal string targetPath;
		internal Action<byte[]> callback;
		internal bool disposed;

		internal abstract byte[] Execute();

		internal Operation(string targetPath, Action<byte[]> callback)
		{
			this.targetPath = targetPath;
			this.callback = callback;
		}

		public void Dispose()
		{
			disposed = true;
		}
	}
}
