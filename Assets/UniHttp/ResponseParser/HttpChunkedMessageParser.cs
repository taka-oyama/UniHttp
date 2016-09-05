using UnityEngine;
using System.IO;

namespace UniHttp
{	
	public class HttpChunkedResponseParser : HttpResponseParser
	{
		NetworkStream inputStream;
		this.bufferSize = bufferSize;
		this.timer = new System.Timers.Timer(10.0);

		int readBytes = 0;
		byte[] buffer = new byte[bufferSize];
		MemoryStream tempStream = new MemoryStream(0);



		void ReadMessageBodyWithChunk(Action nextAction)
		{
			ReadTo(LF, msg => {
				int nextBytes = int.Parse(msg, NumberStyles.HexNumber);
			});
		}

	}
}
