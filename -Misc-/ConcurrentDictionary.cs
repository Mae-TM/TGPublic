using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ConcurrentDictionary<TKey, TValue>
{
	private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

	private ReaderWriterLock rwlock = new ReaderWriterLock();

	public int Count
	{
		get
		{
			rwlock.AcquireReaderLock(-1);
			try
			{
				return dictionary.Count;
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			rwlock.AcquireReaderLock(-1);
			try
			{
				return dictionary[key];
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		set
		{
			rwlock.AcquireWriterLock(-1);
			try
			{
				dictionary[key] = value;
			}
			finally
			{
				rwlock.ReleaseWriterLock();
			}
		}
	}

	public bool ContainsKey(TKey key)
	{
		rwlock.AcquireReaderLock(-1);
		try
		{
			return dictionary.ContainsKey(key);
		}
		finally
		{
			rwlock.ReleaseReaderLock();
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		rwlock.AcquireReaderLock(-1);
		try
		{
			return dictionary.TryGetValue(key, out value);
		}
		finally
		{
			rwlock.ReleaseReaderLock();
		}
	}

	public void Add(TKey key, TValue value)
	{
		rwlock.AcquireWriterLock(-1);
		try
		{
			dictionary.Add(key, value);
		}
		finally
		{
			rwlock.ReleaseWriterLock();
		}
	}

	public bool Remove(TKey key)
	{
		rwlock.AcquireWriterLock(-1);
		try
		{
			return dictionary.Remove(key);
		}
		finally
		{
			rwlock.ReleaseWriterLock();
		}
	}

	public override string ToString()
	{
		rwlock.AcquireReaderLock(-1);
		try
		{
			return dictionary.ToString();
		}
		finally
		{
			rwlock.ReleaseReaderLock();
		}
	}

	public IEnumerable<TKey> FindKeys(TValue b)
	{
		rwlock.AcquireReaderLock(-1);
		try
		{
			return from x in dictionary
				where x.Value.Equals(b)
				select x.Key;
		}
		finally
		{
			rwlock.ReleaseReaderLock();
		}
	}

	public IEnumerable<TValue> GetValues()
	{
		rwlock.AcquireReaderLock(-1);
		try
		{
			return dictionary.Select((KeyValuePair<TKey, TValue> kvp) => kvp.Value);
		}
		finally
		{
			rwlock.ReleaseReaderLock();
		}
	}
}
