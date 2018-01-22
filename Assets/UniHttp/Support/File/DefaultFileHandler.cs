using UnityEngine;
using System.IO;

namespace UniHttp
{
	public class DefaultFileHandler : IFileHandler
	{
		public bool Exists(string path)
		{
			return File.Exists(path);
		}

		public byte[] Read(string path)
		{
			using(Stream input = OpenReadStream(path)) {
				MemoryStream output = new MemoryStream();
				byte[] bytes = new byte[64 * 1024];
				int readBytes;
				while((readBytes = input.Read(bytes, 0, bytes.Length)) > 0) {
					output.Write(bytes, 0, readBytes);
				}
				return output.ToArray();
			}
		}

		public void Write(string path, byte[] data)
		{
			FileInfo info = new FileInfo(path);
			FileInfo temp = new FileInfo(info.FullName + ".tmp");
			info.Directory.Create();
			temp.Delete();
			using(Stream output = OpenWriteStream(temp.FullName)) {
				output.Write(data, 0, data.Length);
			}
			info.Delete();
			File.Move(temp.FullName, info.FullName);
		}

		public void Delete(string path)
		{
			File.Delete(path);
		}

		public virtual Stream OpenReadStream(string path)
		{
			return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public virtual Stream OpenWriteStream(string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}
	}
}
