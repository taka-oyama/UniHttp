using UnityEngine;
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

		public void ReadChunks(Stream destination, bool decompress = false)
		{
			int chunkSize = NextChunkSize();
			while(chunkSize > 0) {
				Read(destination, chunkSize, decompress);
				reader.ReadTo(LF);
				chunkSize = NextChunkSize();
			}
		}

		int NextChunkSize()
		{
			string hexStr = reader.ReadTo(LF).Trim();
			return int.Parse(hexStr, NumberStyles.HexNumber);
		}

		public void Read(Stream destination, int contentLength, bool decompress = false)
		{
			if(decompress) {
				reader.DecompressAndCopyTo(destination, contentLength);
			} else {
				reader.CopyTo(destination, contentLength);
			}
		}
	}
}