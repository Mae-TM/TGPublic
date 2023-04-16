using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T> : IEnumerable<T>, IEnumerable
{
	private readonly Heap<(T, float)> heap = new MaxHeap<(T, float)>(Compare);

	public int Count => heap.Count;

	public void Add(T element, float priority)
	{
		heap.Add((element, priority));
	}

	public float TopPriority()
	{
		return heap.GetDominating().Item2;
	}

	public T Pop()
	{
		return heap.ExtractDominating().Item1;
	}

	private static int Compare((T, float) x, (T, float) y)
	{
		return Comparer<float>.Default.Compare(y.Item2, x.Item2);
	}

	public IEnumerator<(T, float)> GetEnumerator()
	{
		return heap.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return heap.Select(((T, float) value) => value.Item1).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
