using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace UniHttp
{
	internal sealed class GzipDecompressStream : BaseStream
	{
		internal GzipDecompressStream(Stream compressedStream, bool leaveOpen = false) : base(null)
		{
			this.stream = new GZipStream(compressedStream, CompressionMode.Decompress, leaveOpen);
		}
	}
}
