using System.Collections.Generic;

namespace Quest.NET.Utils;

public static class Extensions
{
	public static int HasChanged<T>(this IEnumerable<T> first, IEnumerable<T> second)
	{
		return first.HasChanged(second, Comparer<T>.Default);
	}

	public static int HasChanged<T>(this IEnumerable<T> first, IEnumerable<T> second, Comparer<T> comparer)
	{
		using IEnumerator<T> enumerator = first.GetEnumerator();
		using IEnumerator<T> enumerator2 = second.GetEnumerator();
		int num;
		do
		{
			bool flag = enumerator.MoveNext();
			bool flag2 = enumerator2.MoveNext();
			if (!flag && !flag2)
			{
				return 0;
			}
			if (!flag)
			{
				return -1;
			}
			if (!flag2)
			{
				return 1;
			}
			num = comparer.Compare(enumerator.Current, enumerator2.Current);
		}
		while (num == 0);
		return num;
	}
}
