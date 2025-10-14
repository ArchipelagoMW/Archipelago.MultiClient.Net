using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NET35
using System.Text.RegularExpressions;
#endif

namespace Archipelago.MultiClient.Net.Extensions
{
	static class GuidExtensions
	{
		internal static bool TryParseNGuid(this string s, out Guid g)
		{
#if NET35
			var format = new Regex("^[A-Fa-f0-9]{32}$");
			var match = format.Match(s);
			if (match.Success)
			{
				g = new Guid(s);
				return true;
			}
			else
			{
				g = Guid.Empty;
				return false;
			}
#else
			return Guid.TryParseExact(s, "N", out g);
#endif
		}
	}
}
