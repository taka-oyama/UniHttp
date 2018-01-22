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

		public void Write(string filePath, byte[] data)
		{
			string tempPath = filePath + ".tmp";
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			File.Delete(tempPath);
			using(Stream output = OpenWriteStream(tempPath)) {
				output.Write(data, 0, data.Length);
			}
			File.Delete(filePath);
			File.Move(tempPath, filePath);
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
