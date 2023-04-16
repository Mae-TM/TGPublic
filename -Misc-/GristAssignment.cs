using System.Collections.Generic;
using RandomExtensions;

public static class GristAssignment
{
	private static readonly SortedList<Aspect, List<Aspect>> planets = new SortedList<Aspect, List<Aspect>>();

	public static void Add(Aspect aspect, List<Aspect> planet)
	{
		planets.Add(aspect, planet);
		Reassign();
	}

	public static void Remove(List<Aspect> planet)
	{
		planets.RemoveAt(planets.IndexOfValue(planet));
	}

	private static void Reassign()
	{
		foreach (KeyValuePair<Aspect, List<Aspect>> planet in planets)
		{
			planet.Value.Clear();
			planet.Value.Add(planet.Key);
		}
		AssignUnclaimed();
		foreach (List<Aspect> value in planets.Values)
		{
			value.TrimExcess();
		}
	}

	private static void AssignUnclaimed()
	{
		Stack<Aspect> stack = new Stack<Aspect>(GetUnclaimed());
		List<List<Aspect>> list = new List<List<Aspect>>(planets.Values);
		SessionRandom.Seed();
		int count = planets.Count;
		int i = 1;
		while (stack.Count != 0)
		{
			for (; count / i > stack.Count || count % i != 0; i++)
			{
			}
			list.Shuffle();
			for (int j = 0; j < count; j += i)
			{
				Aspect item = stack.Pop();
				for (int k = 0; k < i; k++)
				{
					list[j + k].Add(item);
				}
			}
		}
	}

	private static IEnumerable<Aspect> GetUnclaimed()
	{
		Aspect aspect3 = (Aspect)(-1);
		foreach (Aspect aspect2 in planets.Keys)
		{
			Aspect a = aspect3 + 1;
			while (a < aspect2)
			{
				yield return a;
				Aspect aspect4 = a + 1;
				a = aspect4;
			}
			aspect3 = aspect2;
		}
		Aspect aspect2 = aspect3 + 1;
		while (aspect2 < Aspect.Count)
		{
			yield return aspect2;
			Aspect aspect4 = aspect2 + 1;
			aspect2 = aspect4;
		}
	}
}
