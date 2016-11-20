using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace UniHttp
{
	/// ----------------------------------------------------------------
	/// Format Spec
	/// ----------------------------------------------------------------
	/// Byte:     |    0    |   1~17   | <-      ...     -> | n-32 ~ n |
	/// Contents: | version |    IV    |     ciphertext     |   HMAC   |
	/// ----------------------------------------------------------------
	/// 
	/// 本当は↓が使いたいけど.NET 3.5 以上じゃないと使えない
	/// https://blogs.msdn.microsoft.com/shawnfa/2009/03/17/authenticated-symmetric-encryption-in-net/
	/// 
	/// RNCryptorのKey-based encryptionを参考に造られている
	/// https://github.com/RNCryptor/RNCryptor-Spec/blob/master/RNCryptor-Spec-v3.md
	/// 
	/// 概念説明: Authenticated encryption
	/// https://en.wikipedia.org/wiki/Authenticated_encryption
	/// 
	/// いじる前に Why I hate CBC-MAC をちゃんと読んでね
	/// http://blog.cryptographyengineering.com/2013/02/why-i-hate-cbc-mac.html
	/// 

	public class AuthenticatedEncryption
	{
		enum Version : byte { V0, V1 }

		const int blockSize = 128;
		const int keySize = 256;
		const string salt = "shinystudio";

		/// <summary>
		/// Implemented using AES(CBC) + HMAC
		/// 1. make AES Key and HMAC key from password
		/// 2. Generate a random IV
		/// 3. Encrypt the data with AES key, IV, AES-256, and CBC mode
		/// 4. Pass header and ciphertext to an HMAC function, along with HMAC key
		/// 5. Put these elements together in the format given
		/// </summary>
		/// <param name="encryptionKey">Must be 32 bytes long</param>
		/// <param name="hmacKey">Must be 32 bytes long</param>
		/// <param name="data">Data which will get encrypted</param>
		public static byte[] Encrypt(string password, byte[] data)
		{
			return Encrypt(Encoding.ASCII.GetBytes(password), data);
		}

		public static byte[] Encrypt(byte[] password, byte[] data)
		{
			byte[] derivedKey = DeriveKey(password);
			byte[] aesKey = new byte[32];
			byte[] hmacKey = new byte[32];
			Buffer.BlockCopy(derivedKey, 0, aesKey, 0, aesKey.Length);
			Buffer.BlockCopy(derivedKey, aesKey.Length, hmacKey, 0, hmacKey.Length);

			var aes = PrepareAes(aesKey);
			aes.GenerateIV();

			var aead = new List<byte>();
			aead.Add((byte)Version.V1);
			aead.AddRange(aes.IV);

			var ciphertext = aes.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
			aead.AddRange(ciphertext);

			var hmac = new HMACSHA256(hmacKey).ComputeHash(aead.ToArray());
			aead.AddRange(hmac);

			return aead.ToArray();
		}

		public static byte[] Decrypt(string password, byte[] aead)
		{
			return Decrypt(Encoding.ASCII.GetBytes(password), aead);
		}

		public static byte[] Decrypt(byte[] password, byte[] aead)
		{
			byte[] derivedKey = DeriveKey(password);
			byte[] aesKey = new byte[32];
			byte[] hmacKey = new byte[32];
			Buffer.BlockCopy(derivedKey, 0, aesKey, 0, aesKey.Length);
			Buffer.BlockCopy(derivedKey, aesKey.Length, hmacKey, 0, hmacKey.Length);

			byte[] version = new byte[1];
			Buffer.BlockCopy(aead, 0, version, 0, version.Length);

			byte[] iv = new byte[16];
			Buffer.BlockCopy(aead, version.Length, iv, 0, iv.Length);

			byte[] hmac = new byte[32];
			Buffer.BlockCopy(aead, aead.Length - hmac.Length, hmac, 0, hmac.Length);

			byte[] ciphertext = new byte[aead.Length - (version.Length + iv.Length + hmac.Length)];
			Buffer.BlockCopy(aead, version.Length + iv.Length, ciphertext, 0, ciphertext.Length);

			byte[] hmacRef = new byte[version.Length + iv.Length + ciphertext.Length];
			Buffer.BlockCopy(aead, 0, hmacRef, 0, hmacRef.Length);

			// authenticate
			byte[] computedHmac = new HMACSHA256(hmacKey).ComputeHash(hmacRef);
			if(!computedHmac.SequenceEqual(hmac)) {
				string message = "Authentication mismatch!";
				if(Debug.isDebugBuild) {
					message = string.Format("Authentication mismatch! Expected:{0} | Given:{1}", hmac, computedHmac);
				} 
				throw new CryptographicException(message);
			}

			// decrypt
			var aes = PrepareAes(aesKey);
			aes.IV = iv;
			var data = aes.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);

			return data;
		}

		static RijndaelManaged PrepareAes(byte[] encryptionKey)
		{
			var aes = new RijndaelManaged();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.BlockSize = blockSize;
			aes.KeySize = keySize;
			aes.Key = encryptionKey;
			return aes;
		}

		static byte[] DeriveKey(byte[] password)
		{
			return new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(salt), 1000).GetBytes(64);
		}
	}
}