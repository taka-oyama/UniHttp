using System.IO;
using UnityEngine;

namespace UniHttp
{
	internal sealed class CacheStream : BaseStream
	{
		internal CacheStream(Stream stream, int bufferSize) : base(stream, bufferSize)
		{
		}
	}
}
