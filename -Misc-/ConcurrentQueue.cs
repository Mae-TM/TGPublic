using System.Collections.Generic;
using System.Threading;

public class ConcurrentQueue<T>
{
	private Queue<T> queue = new Queue<T>();

	private ReaderWriterLock rwlock = new ReaderWriterLock();

	public int Count
	{
		get
		{
			rwlock.AcquireReaderLock(-1);
			try
			{
				return queue.Count;
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
	}

	public void Enqueue(T item)
	{
		rwlock.AcquireWriterLock(-1);
		try
		{
			queue.Enqueue(item);
		}
		finally
		{
			rwlock.ReleaseWriterLock();
		}
	}

	public T Dequeue()
	{
		rwlock.AcquireWriterLock(-1);
		try
		{
			return queue.Dequeue();
		}
		finally
		{
			rwlock.ReleaseWriterLock();
		}
	}
}
