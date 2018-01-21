using System.IO;
using System.Threading;
using UnityEngine;

namespace UniHttp
{
	internal sealed class CacheStream : BaseStream
	{
		Mutex mutex;

		internal CacheStream(Stream stream, Mutex mutex) : base(stream)
		{
			this.mutex = mutex;
		}

		protected override void Dispose(bool disposing)
		{
			mutex.ReleaseMutex();
			base.Dispose(disposing);
		}
	}
}
