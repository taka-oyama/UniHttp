using System.IO;

namespace UniHttp
{
	public interface IFileHandler
	{
		bool Exists(string path);

		void Write(string path, byte[] data);

		void WriteObject<T>(string path, T obj) where T : class;

		FileStream OpenWriteStream(string path);

		byte[] Read(string path);

		FileStream OpenReadStream(string path);

		T ReadObject<T>(string path) where T : class;
	}
}
