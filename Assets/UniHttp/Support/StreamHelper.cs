using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.IO;
using Unity.IO.Compression;

namespace UniHttp
{
	internal class StreamHelper
	{
		internal static void CopyTo(Stream source, Stream destination, int count, int bufferSize = 1024, bool decompress = false)
		{
			byte[] buffer = new byte[bufferSize];
			int remainingBytes = count;
			int readBytes = 0;

			if(decompress) {
				using(GZipStream gzipStream = new GZipStream(source, CompressionMode.Decompress)) {
					while(remainingBytes > 0) {
						while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
							destination.Write(buffer, 0, readBytes);
							remainingBytes -= readBytes;
						}
					}
				}
			} else {
				while(remainingBytes > 0) {
					readBytes = source.Read(buffer, 0, Math.Min(buffer.Length, remainingBytes));
					destination.Write(buffer, 0, readBytes);
					remainingBytes -= readBytes;
				}
			}
		}

		internal static int SkipTo(Stream source, params char[] stoppers)
		{
			int b;
			int count = 0;
			do {
				b = source.ReadByte();
				if(b == -1) break;
				count += 1;
			} while(stoppers.All(s => b != (int)s));
			return count;
		}

		internal static int ReadTo(Stream source, Stream destination, params char[] stoppers)
		{
			int b;
			int count = 0;
			do {
				b = source.ReadByte();
				destination.WriteByte((byte)b);
				if(b == -1) break;
				count += 1;
			} while(stoppers.All(s => b != (int)s));
			return count;
		}


	}
}
