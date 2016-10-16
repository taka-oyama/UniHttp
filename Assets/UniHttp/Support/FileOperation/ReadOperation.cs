using UnityEngine;
using System;
using System.IO;

namespace UniHttp.FileOperation
{
	internal class ReadOperation : Operation
	{
		internal ReadOperation(string targetPath, Action<byte[]> callback) : base(targetPath, callback)
		{
		}

		internal override byte[] Execute()
		{
			if(!disposed) {
				return File.ReadAllBytes(targetPath);
			} else {
				return null;
			}
		}
	}
}
