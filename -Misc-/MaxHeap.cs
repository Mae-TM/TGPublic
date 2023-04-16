using System.Collections.Generic;

public class MaxHeap<T> : Heap<T>
{
	public MaxHeap(Compare comparer)
		: base(comparer)
	{
	}

	public MaxHeap(IEnumerable<T> collection, Compare comparer)
		: base(collection, comparer)
	{
	}

	protected override bool Dominates(T x, T y)
	{
		return base.Comparer(x, y) >= 0;
	}
}
