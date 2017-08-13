using UnityEngine;
using System;

namespace UniHttp
{
	public class SecureFileHandler : DefaultFileHandler
	{
		string password;

		public SecureFileHandler(string password) : base()
		{
			this.password = password;
		}

		public override void Write(string path, byte[] data)
		{			
			base.Write(path, AuthenticatedEncryption.Encrypt(password, data));
		}

		public override byte[] Read(string path)
		{
			return AuthenticatedEncryption.Decrypt(password, base.Read(path));
		}
	}
}
