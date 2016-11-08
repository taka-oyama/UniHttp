using UnityEngine;
using System.Collections.Generic;

namespace UniHttp
{	
	public class ContentMimeMap : Dictionary<string, IContentSerializer>
	{
		public static ContentMimeMap Default
		{
			get {
				return new ContentMimeMap() {
					{ "application/json", new JsonSerializer() },
				};
			}
		}
	}
}
