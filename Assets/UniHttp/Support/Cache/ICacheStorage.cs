using UnityEngine;
using System;
using System.IO;

namespace UniHttp
{
	public interface ICacheStorage
	{
		void Write(DirectoryInfo baseDirectory, Uri uri, byte[] data);
		byte[] Read(Uri uri);
	}
}
