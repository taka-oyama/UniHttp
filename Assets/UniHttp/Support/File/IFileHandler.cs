using System.IO;

namespace UniHttp
{
	public interface IFileHandler
	{
		bool Exists(string path);

		void Write(string path, byte[] data);

		byte[] Read(string path);

		FileStream OpenWriteStream(string path);

		FileStream OpenReadStream(string path);

		void WriteObject<T>(string path, T obj) where T : class;

		T ReadObject<T>(string path) where T : class;
	}
}
