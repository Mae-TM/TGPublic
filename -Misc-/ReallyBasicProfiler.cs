using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using QFSW.QC;

public class ReallyBasicProfiler : AbstractSingletonManager<ReallyBasicProfiler>
{
	public bool Enabled { get; set; }

	public Dictionary<string, List<MetricTracker>> Metrics { get; set; }

	public ReallyBasicProfiler()
	{
		Metrics = new Dictionary<string, List<MetricTracker>>();
	}

	public List<MethodProfile> GenerateReport()
	{
		List<MethodProfile> list = new List<MethodProfile>();
		foreach (KeyValuePair<string, List<MetricTracker>> metric in Metrics)
		{
			int count = metric.Value.Count;
			double averageExecutionTime = metric.Value.Where((MetricTracker m) => !m.stopwatch.IsRunning).Average((MetricTracker m) => m.stopwatch.ElapsedMilliseconds);
			long longestExecutionTime = metric.Value.Where((MetricTracker m) => !m.stopwatch.IsRunning).Max((MetricTracker m) => m.stopwatch.ElapsedMilliseconds);
			DateTime startTime = metric.Value.Select((MetricTracker c, int i) => new
			{
				character = c,
				index = i
			}).First(m => !m.character.stopwatch.IsRunning && m.character.stopwatch.ElapsedMilliseconds == longestExecutionTime).character.startTime;
			list.Add(new MethodProfile
			{
				averageExecutionTime = averageExecutionTime,
				longestExecutionTime = longestExecutionTime,
				longestExecutionTimestamp = startTime,
				methodCalls = count,
				methodName = metric.Key
			});
		}
		return list;
	}

	public void Clear()
	{
		Metrics.Clear();
	}

	private string GetMethodContextName(MethodBase method)
	{
		if (method.DeclaringType.GetInterfaces().Any((Type i) => i == typeof(IAsyncStateMachine)))
		{
			Type generatedType = method.DeclaringType;
			MethodInfo methodInfo = generatedType.DeclaringType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Single((MethodInfo m) => CustomAttributeExtensions.GetCustomAttribute<AsyncStateMachineAttribute>(m)?.StateMachineType == generatedType);
			return methodInfo.DeclaringType.Name + "." + methodInfo.Name;
		}
		return method.DeclaringType.Name + "." + method.Name;
	}

	public MetricTracker Track(MethodBase mb)
	{
		if (Enabled)
		{
			QuantumConsole.Instance.LogToConsole("Profiling method " + mb.Name + "...");
		}
		string methodContextName = GetMethodContextName(mb);
		if (!Metrics.ContainsKey(methodContextName))
		{
			Metrics.Add(methodContextName, new List<MetricTracker>());
		}
		MetricTracker metricTracker = new MetricTracker(methodContextName, Enabled);
		Metrics[methodContextName].Add(metricTracker);
		return metricTracker;
	}
}
