using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace UniHttp
{
	public class CryptoFileHandler : DefaultFileHandler
	{
		readonly byte[] aesKey;

		public CryptoFileHandler(string salt, string password)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(salt);
			Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes(password, bytes, 1000);
			this.aesKey = derivedBytes.GetBytes(32);
		}

		public override Stream OpenReadStream(string path)
		{
			RijndaelManaged aes = GenerateAesContext();
			Stream stream = base.OpenReadStream(path);
			try {
				byte[] bytes = new byte[16];
				stream.Read(bytes, 0, bytes.Length);
				aes.IV = bytes;
				ICryptoTransform decryptor = aes.CreateDecryptor();
				return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
			}
			catch {
				stream.Dispose();
				throw;
			}
		}

		public override Stream OpenWriteStream(string path)
		{
			RijndaelManaged aes = GenerateAesContext();
			Stream stream = base.OpenWriteStream(path);
			try {
				aes.GenerateIV();
				stream.Write(aes.IV, 0, aes.IV.Length);
				ICryptoTransform encryptor = aes.CreateEncryptor();
				return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
			}
			catch {
				stream.Dispose();
				throw;
			}
		}

		RijndaelManaged GenerateAesContext()
		{
			return new RijndaelManaged() {
				BlockSize = 128,
				KeySize = 128,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				Key = aesKey,
			};
		}
	}
}
