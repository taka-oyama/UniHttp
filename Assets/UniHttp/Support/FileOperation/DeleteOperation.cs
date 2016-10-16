using UnityEngine;
using System;
using System.IO;

namespace UniHttp.FileOperation
{
	internal class DeleteOperation : Operation
	{
		internal DeleteOperation(string targetPath) : this(targetPath, null)
		{
		}

		internal DeleteOperation(string targetPath, Action<byte[]> callback) : base(targetPath, callback)
		{
		}

		internal override byte[] Execute()
		{
			if(!disposed) {
				File.Delete(targetPath);
			}
			return null;
		}
	}
}
