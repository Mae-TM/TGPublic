using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RandomExtensions;

public static class RandomExtensions
{
	public static void Shuffle<T>(this IList<T> array)
	{
		int count = array.Count;
		while (count > 1)
		{
			int index = UnityEngine.Random.Range(0, count--);
			T value = array[count];
			array[count] = array[index];
			array[index] = value;
		}
	}

	public static string ToSHA256(this string text)
	{
		using SHA256Managed sHA256Managed = new SHA256Managed();
		return BitConverter.ToString(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
	}
}
