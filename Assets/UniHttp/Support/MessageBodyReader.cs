﻿using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text;

namespace UniHttp
{
	public class MessageBodyReader
	{
		const char LF = '\n';

		StreamReader reader;

		public MessageBodyReader(StreamReader reader)
		{
			this.reader = reader;
		}

		public byte[] ReadChunks(bool decompress = false)
		{
			MemoryStream chunk = new MemoryStream(0);
			int chunkSize = NextChunkSize();

			while(chunkSize > 0) {
				byte[] bytes = Read(chunkSize, decompress);
				chunk.Write(bytes, 0, bytes.Length);
				reader.ReadUpTo(LF);
				chunkSize = NextChunkSize();
			}
			return chunk.ToArray();
		}

		int NextChunkSize()
		{
			string hexStr = reader.ReadUpTo(LF).Trim();
			return int.Parse(hexStr, NumberStyles.HexNumber);
		}

		public byte[] Read(int contentLength, bool decompress = false)
		{
			if(decompress) {
				return reader.ReadAndDecompress(contentLength);
			} else {
				return reader.Read(contentLength);
			}
		}
	}
}