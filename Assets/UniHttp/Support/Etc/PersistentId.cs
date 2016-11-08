using UnityEngine;
using System.IO;
using System;

internal sealed class PersistentId
{
	FileInfo info;
	string id;

	internal PersistentId(FileInfo info)
	{
		this.info = info;
		Fetch();
	}

	internal string Fetch()
	{
		if(id != null) {
			return id;
		}
		if(info.Exists) {
			id = File.ReadAllText(info.FullName);
			return id;
		}
		id = new Guid().ToString("N");
		File.WriteAllText(info.FullName, id);
		return id;
	}
}
