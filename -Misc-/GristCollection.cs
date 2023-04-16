using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GristCollection : IEnumerable<(int index, int value)>, IEnumerable
{
	public delegate void OnChangeHandler(int index, int before, int after);

	public OnChangeHandler OnGristChange;

	private readonly int[] array = new int[63];

	public int this[int index]
	{
		get
		{
			return array[index];
		}
		set
		{
			int num = array[index];
			if (num != value)
			{
				OnGristChange?.Invoke(index, num, value);
				array[index] = value;
			}
		}
	}

	public int this[Grist.SpecialType type]
	{
		get
		{
			return this[Grist.GetIndex(type)];
		}
		set
		{
			this[Grist.GetIndex(type)] = value;
		}
	}

	public int this[int level, Aspect type]
	{
		get
		{
			return this[Grist.GetIndex(level, type)];
		}
		set
		{
			this[Grist.GetIndex(level, type)] = value;
		}
	}

	public IEnumerator<(int index, int value)> GetEnumerator()
	{
		int i = 0;
		while (i < 63)
		{
			int num = this[i];
			if (num != 0)
			{
				yield return (i, num);
			}
			int num2 = i + 1;
			i = num2;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool CanPay(GristCollection other)
	{
		return other.All(((int index, int value) pair) => this[pair.index] >= pair.value);
	}

	public void Add(GristCollection other)
	{
		foreach (var (index, num) in other)
		{
			this[index] += num;
		}
	}

	public void Subtract(GristCollection other)
	{
		foreach (var (index, num) in other)
		{
			this[index] -= num;
		}
	}

	public int[] Save()
	{
		return array.ToArray();
	}

	public void Load(int[] data)
	{
		Array.Copy(data, array, Math.Min(data.Length, array.Length));
	}

	public void Save(Stream stream)
	{
		int[] array = this.array;
		for (int i = 0; i < array.Length; i++)
		{
			HouseLoader.writeInt(array[i], stream);
		}
	}

	public void Load(Stream stream)
	{
		for (int i = 0; i < 63; i++)
		{
			array[i] = HouseLoader.readInt(stream);
		}
	}
}
