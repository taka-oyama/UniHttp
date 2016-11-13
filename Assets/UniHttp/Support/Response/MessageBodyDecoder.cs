using UnityEngine;
using System.IO;
using System.IO.Compression;

namespace UniHttp
{
	internal sealed class MessageBodyDecoder
	{
		HttpResponse response;
		int bufferSize;

		internal MessageBodyDecoder(HttpResponse response, int bufferSize = 1024)
		{
			this.response = response;
			this.bufferSize = bufferSize;
		}

		internal byte[] Decode(MemoryStream source)
		{
			if(response.Headers.Exist("Content-Encoding", "gzip")) {
				return DecodeAsGzip(source);
			} else {
				return source.ToArray();
			}
		}

		byte[] DecodeAsGzip(MemoryStream source)
		{
			byte[] buffer = new byte[bufferSize];
			int readBytes = 0;

			source.Seek(0, SeekOrigin.Begin);

			using(var destination = new MemoryStream()) {
				using(var gzipStream = new GZipStream(source, CompressionMode.Decompress)) {
					while((readBytes = gzipStream.Read(buffer, 0, buffer.Length)) > 0) {
						destination.Write(buffer, 0, readBytes);
					}
					return destination.ToArray();
				}
			}
		}
	}
}
