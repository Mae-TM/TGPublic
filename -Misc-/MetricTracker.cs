using System;
using System.Diagnostics;
using QFSW.QC;

public sealed class MetricTracker : IDisposable
{
	private readonly string _methodName;

	public readonly Stopwatch stopwatch;

	public readonly DateTime startTime;

	private bool _enabled;

	internal MetricTracker(string methodName, bool enabled = true)
	{
		_methodName = methodName;
		_enabled = enabled;
		if (enabled)
		{
			startTime = DateTime.UtcNow;
			stopwatch = Stopwatch.StartNew();
		}
	}

	void IDisposable.Dispose()
	{
		if (_enabled)
		{
			stopwatch.Stop();
			QuantumConsole.Instance.LogToConsole($"Method {_methodName} ran for {stopwatch.ElapsedMilliseconds}ms.");
		}
	}
}
