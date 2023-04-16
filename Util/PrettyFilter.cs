using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Util;

public static class PrettyFilter
{
	private static List<Regex> patternListCache = new List<Regex>();

	private static string[] patternList = new string[20]
	{
		"\\bn+[\\W_]{0,4}[!i\\/?1\\\\]+[\\W_]{0,4}[qgb]+[\\W_]{0,4}[qgb]?[\\W_]{0,4}(?:[e3][\\W_]{0,4}r|a)(?!ia|al)s*\\b", "\\bbean(?:er)?\\b", "\\bf+[a4]+g+s*\\b", "\\bf+[a4]+g+[oiy]+t*s*\\b", "\\bf[a4]g+ing\\b", "\\bf[a4]gg[0o]tc[0o]ck\\b", "\\b(?:r[e3]|l[1i]b|f[a4]g)t[a4]rd\\b", "\\bfaig\\b", "\\bfaigt\\b", "\\bn+[3e]+g+r[0o]+\\b",
		"\\bn[3e][0o]n[4a][s2z][1i]\\b", "\\bn[i1]g[ -]n[o0]g\\b", "\\bnigl[e3]t\\b", "\\br[4a]+p[3e]+[rdy]?\\b", "\\br[a4]p[1i]ng\\b", "\\bs[a4]ndn[i1][qgb]+[3e]+r\\b", "\\bsanger\\b", "\\b[7t]r[a4]n+(?:y+|[1i][3e])\\b", "\\b[ck]\\s*u\\s*c\\s*k\\b", "\\bc(?:h|\\)\\()[1i]nk\\b"
	};

	public static void TryUpdateCache()
	{
		if (patternListCache.Count == 0)
		{
			patternListCache.Capacity = patternList.Length;
			string[] array = patternList;
			foreach (string pattern in array)
			{
				patternListCache.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
			}
		}
	}

	public static bool IsNotPretty(string str)
	{
		TryUpdateCache();
		foreach (Regex item in patternListCache)
		{
			if (item.IsMatch(str))
			{
				return true;
			}
		}
		return false;
	}

	[Obsolete("We should be more hostile towards users who use slurs and other awful language. Consider replacing with a more sophisicated call to IsNotPretty().")]
	public static string CensorString(string str)
	{
		TryUpdateCache();
		foreach (Regex item in patternListCache)
		{
			str = item.Replace(str, "****");
		}
		return str;
	}
}
