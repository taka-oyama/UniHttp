using UnityEngine;
using System;
using System.IO;

namespace UniHttp.FileOperation
{
	internal class WriteOperation : Operation
	{
		internal byte[] data;

		internal WriteOperation(string targetPath, byte[] data) : this(targetPath, data, null)
		{
		}

		internal WriteOperation(string targetPath, byte[] data, Action<byte[]> callback) : base(targetPath, callback)
		{
			this.data = data;
		}

		internal override byte[] Execute()
		{
			if(!disposed) {
				File.WriteAllBytes(targetPath, data);
				return data;
			} else {
				return null;
			}
		}
	}
}
