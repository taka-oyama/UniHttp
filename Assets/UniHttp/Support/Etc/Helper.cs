using UnityEngine;
using System;
using System.Globalization;

namespace UniHttp
{
	public class Helper
	{
		public static DateTime ParseDate(string rfc1123FormattedString)
		{
			return DateTime.ParseExact(
				rfc1123FormattedString,
				CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern,
				CultureInfo.InvariantCulture
			);
		}
	}
}
