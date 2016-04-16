using System;
using System.Collections.Generic;
using System.Linq;

namespace NoesisLabs.Elve.Kodi
{
	public static class Extensions
	{
		public static IEnumerable<int> AllIndexesOf(this string str, string searchstring)
		{
			int minIndex = str.IndexOf(searchstring);
			while (minIndex != -1)
			{
				yield return minIndex;
				minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length);
			}
		}

		public static int IndexOfLastTokenEnd(this string s)
		{
			var startBracketPositions = s.AllIndexesOf("{");
			var endBracketPositions = s.AllIndexesOf("}");

			var orderedBrackets = startBracketPositions.Select(p => new KeyValuePair<int, int>(p, 1)).Concat(endBracketPositions.Select(p => new KeyValuePair<int, int>(p, -1))).OrderBy(kvp => kvp.Key);

			var sums = orderedBrackets.Select((b, index) => orderedBrackets.Take(index + 1).Sum(kvp => kvp.Value));

			int lastIndex = sums.ToList().LastIndexOf(0);

			return (lastIndex == -1) ? -1 : orderedBrackets.ToList()[lastIndex].Key;
		}
	}
}