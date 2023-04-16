using System.Collections.Generic;

public class CacheDictionary<TKey, TValue>
{
	private readonly Dictionary<TKey, TValue> storage;

	private readonly Queue<TKey> keys;

	private readonly int capacity;

	public TValue this[TKey key]
	{
		set
		{
			if (!storage.ContainsKey(key))
			{
				if (storage.Count >= capacity)
				{
					storage.Remove(keys.Dequeue());
				}
				storage.Add(key, value);
				keys.Enqueue(key);
			}
		}
	}

	public CacheDictionary(int capacity)
	{
		this.capacity = capacity;
		storage = new Dictionary<TKey, TValue>(capacity);
		keys = new Queue<TKey>(capacity);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return storage.TryGetValue(key, out value);
	}

	public void Clear()
	{
		storage.Clear();
		keys.Clear();
	}
}
