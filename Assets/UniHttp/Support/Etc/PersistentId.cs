using UnityEngine;
using System.IO;
using System;

public sealed class PersistentId
{
	FileInfo info;
	string id;

	internal PersistentId(FileInfo info)
	{
		this.info = info;
		Fetch();
	}

	public string Fetch()
	{
		if(id != null) {
			return id;
		}
		if(info.Exists) {
			id = File.ReadAllText(info.FullName);
			return id;
		}
		id = Guid.NewGuid().ToString("N");
		info.Directory.Create();
		File.WriteAllText(info.FullName, id);
		return id;
	}
}
