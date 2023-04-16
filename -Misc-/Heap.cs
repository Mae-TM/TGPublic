using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Heap<T> : IEnumerable<T>, IEnumerable
{
	public delegate int Compare(T a, T b);

	private const int InitialCapacity = 0;

	private const int GrowFactor = 2;

	private const int MinGrow = 1;

	private T[] _heap = new T[0];

	public int Count { get; private set; }

	public int Capacity { get; private set; }

	protected Compare Comparer { get; private set; }

	protected abstract bool Dominates(T x, T y);

	protected Heap(Compare comparer)
		: this(Enumerable.Empty<T>(), comparer)
	{
	}

	protected Heap(IEnumerable<T> collection, Compare comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		Comparer = comparer;
		foreach (T item in collection)
		{
			if (Count == Capacity)
			{
				Grow();
			}
			_heap[Count++] = item;
		}
		for (int num = Parent(Count - 1); num >= 0; num--)
		{
			BubbleDown(num);
		}
	}

	public void Add(T item)
	{
		if (Count == Capacity)
		{
			Grow();
		}
		_heap[Count++] = item;
		BubbleUp(Count - 1);
	}

	public T GetDominating()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Heap is empty");
		}
		return _heap[0];
	}

	public T ExtractDominating()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Heap is empty");
		}
		T result = _heap[0];
		Count--;
		Swap(Count, 0);
		BubbleDown(0);
		return result;
	}

	public int IndexOf(Predicate<T> predicate)
	{
		for (int i = 0; i < Count; i++)
		{
			if (predicate(_heap[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private void BubbleUp(int i)
	{
		if (i != 0 && !Dominates(_heap[Parent(i)], _heap[i]))
		{
			Swap(i, Parent(i));
			BubbleUp(Parent(i));
		}
	}

	private void BubbleDown(int i)
	{
		int num = Dominating(i);
		if (num != i)
		{
			Swap(i, num);
			BubbleDown(num);
		}
	}

	private int Dominating(int i)
	{
		int dominatingNode = i;
		dominatingNode = GetDominating(YoungChild(i), dominatingNode);
		return GetDominating(OldChild(i), dominatingNode);
	}

	private int GetDominating(int newNode, int dominatingNode)
	{
		if (newNode < Count && !Dominates(_heap[dominatingNode], _heap[newNode]))
		{
			return newNode;
		}
		return dominatingNode;
	}

	private void Swap(int i, int j)
	{
		T val = _heap[i];
		_heap[i] = _heap[j];
		_heap[j] = val;
	}

	private static int Parent(int i)
	{
		return (i + 1) / 2 - 1;
	}

	private static int YoungChild(int i)
	{
		return (i + 1) * 2 - 1;
	}

	private static int OldChild(int i)
	{
		return YoungChild(i) + 1;
	}

	private void Grow()
	{
		int num = Capacity * 2 + 1;
		T[] array = new T[num];
		Array.Copy(_heap, array, Capacity);
		_heap = array;
		Capacity = num;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _heap.Take(Count).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
