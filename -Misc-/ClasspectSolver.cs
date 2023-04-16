using System;
using System.Collections.Generic;
using System.Linq;
using HungarianAlgorithm;
using HungarianAlgorithm.Extensions;
using RandomExtensions;
using Steamworks;
using UnityEngine;

public class ClasspectSolver
{
	private class Player
	{
		public int[] role;

		public int[] aspect;

		public readonly Action<Classpect> OnDone;

		public Player(Action<Classpect> onDone)
		{
			OnDone = onDone;
			role = new int[12];
			aspect = new int[12];
		}

		public void SetClasspect(int[] role, int[] aspect)
		{
			Simplify(role);
			Simplify(aspect);
			this.role = role;
			this.aspect = aspect;
		}

		private static void Simplify(int[] array)
		{
			int num = array.Aggregate(array[0], Gcd);
			if (num != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] /= num;
				}
			}
		}

		public static int[] GetClasspectScores(Player player)
		{
			int[] array = player.aspect.ToArray();
			int[] array2 = player.role.ToArray();
			Normalize(new int[2][] { array, array2 });
			int[] array3 = new int[array.Length * array2.Length];
			int num = 0;
			int[] array4 = array;
			foreach (int num2 in array4)
			{
				int[] array5 = array2;
				foreach (int num3 in array5)
				{
					array3[num++] = num2 + num3;
				}
			}
			return array3;
		}
	}

	public bool avoidDuplicateClasses = true;

	public bool avoidDuplicateAspects = true;

	public bool avoidDuplicateClasspects = true;

	private readonly IDictionary<SteamId, Player> players = new Dictionary<SteamId, Player>();

	public void AddPlayer(SteamId user, Action<Classpect> onDone)
	{
		players.Add(user, new Player(onDone));
	}

	public void RemovePlayer(SteamId user)
	{
		players.Remove(user);
	}

	public void Clear()
	{
		players.Clear();
	}

	public void SetClasspect(SteamId user, int[] role, int[] aspect)
	{
		players[user].SetClasspect(role, aspect);
		Refresh();
	}

	private static int Gcd(int a, int b)
	{
		while (b != 0)
		{
			int num = b;
			b = a % b;
			a = num;
		}
		return a;
	}

	private static int Lcm(int a, int b)
	{
		return a / Gcd(a, b) * b;
	}

	private static int[][] Shuffle(int[][] matrix, out int[] c, out int[] r)
	{
		c = Enumerable.Range(0, matrix.Length).ToArray();
		r = Enumerable.Range(0, matrix[0].Length).ToArray();
		c.Shuffle();
		r.Shuffle();
		int[][] array = new int[matrix.Length][];
		for (int i = 0; i < matrix.Length; i++)
		{
			array[c[i]] = new int[matrix[i].Length];
			for (int j = 0; j < matrix[i].Length; j++)
			{
				array[c[i]][j] = matrix[i][r[j]];
			}
		}
		return array;
	}

	private static IEnumerable<int> Unshuffle(int[] list, int[] c, int[] r)
	{
		return list.Select((int _, int i) => r[list[c[i]]]);
	}

	private static void Normalize(IReadOnlyList<int[]> matrix)
	{
		int[] array = matrix.Select((int[] row) => row.Sum()).ToArray();
		int num = array.Where((int s) => s != 0).Aggregate(1, Lcm);
		for (int i = 0; i < matrix.Count; i++)
		{
			if (array[i] != 0)
			{
				int num2 = num / array[i];
				for (int j = 0; j < matrix[i].Length; j++)
				{
					matrix[i][j] *= -num2;
				}
			}
		}
	}

	private static IEnumerable<int> Solve(IEnumerable<int[]> values)
	{
		int[] c;
		int[] r;
		int[][] array = Shuffle(values.ToArray(), out c, out r);
		Normalize(array);
		return Unshuffle(array.SquareArray().FindAssignments(), c, r);
	}

	private static int Solve(int[] values)
	{
		int max = values.Max();
		var array = (from list in values.Select((int c, int i) => new
			{
				character = c,
				index = i
			})
			where list.character == max
			select list).ToArray();
		return array[UnityEngine.Random.Range(0, array.Length)].index;
	}

	private IEnumerable<Classpect> GetClasspects()
	{
		if (avoidDuplicateClasspects)
		{
			return Solve(players.Values.Select(Player.GetClasspectScores)).Select(Classpect.Convert);
		}
		IEnumerable<int> second = ((!avoidDuplicateAspects) ? players.Values.Select((Player player) => Solve(player.aspect)) : Solve(players.Values.Select((Player player) => player.aspect)));
		IEnumerable<int> first = ((!avoidDuplicateClasses) ? players.Values.Select((Player player) => Solve(player.role)) : Solve(players.Values.Select((Player player) => player.role)));
		return first.Zip(second, Classpect.Create);
	}

	private void Refresh()
	{
		SessionRandom.Seed();
		IEnumerable<Classpect> classpects = GetClasspects();
		foreach (var (player2, obj) in players.Values.Zip(classpects, Tuple.Create))
		{
			player2.OnDone(obj);
		}
	}
}
