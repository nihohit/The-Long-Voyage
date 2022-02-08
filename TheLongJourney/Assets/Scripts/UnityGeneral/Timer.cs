using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Base;

namespace Assets.Scripts.UnityBase
{
	/// <summary>
	/// A static class that holds all the timers running throughout the program.
	/// </summary>
	public class Timer
	{
		#region singleton

		public static Timer Instance
		{
			get
			{
				return Singleton<Timer>.Instance;
			}
		}

		private Timer()
		{ }

		#endregion singleton

		#region TimerContext

		public class TimerContext
		{
			private readonly Stopwatch r_stopwatch;
			public string Name { get; private set; }

			public TimerContext(string name)
			{
				Name = name;
				r_stopwatch = new Stopwatch();
			}

			public void Start()
			{
				r_stopwatch.Start();
			}

			public long Stop()
			{
				r_stopwatch.Stop();
				return r_stopwatch.ElapsedTicks;
			}
		}

		#endregion TimerContext

		private readonly IDictionary<string, List<long>> r_results = new Dictionary<string, List<long>>();

		public TimerContext StartTiming(string operation)
		{
			var context = new TimerContext(operation);
#if DEBUG
			context.Start();
#endif
			return context;
		}

		public void StopTiming(TimerContext context)
		{
#if DEBUG
			lock (r_results)
			{
				var result = context.Stop();
				UnityEngine.Debug.Log("{0} ran for {1}".FormatWith(context.Name, result));
				r_results.TryGetOrAdd(context.Name, () => new List<long>()).Add(result);
			}
#endif
		}

		public void TimedAction(Action action, string operation)
		{
			var context = StartTiming(operation);
			try
			{
				action();
			}
			finally
			{
				StopTiming(context);
			}
		}

		public T TimedAction<T>(Func<T> action, string operation)
		{
			var context = StartTiming(operation);
			try
			{
				return action();
			}
			finally
			{
				StopTiming(context);
			}
		}

		public IEnumerable<string> Results()
		{
			foreach (var opResult in r_results)
			{
				opResult.Value.Sort();
				yield return ("Timing {0}: total {1}ms, count {2}, average {3}ms, median {4}ms, min {5}ms, max {6}ms".FormatWith(
					opResult.Key, 
					TimeSpan.FromTicks(opResult.Value.Sum()).TotalMilliseconds, 
					opResult.Value.Count,
					TimeSpan.FromTicks(Convert.ToInt64(opResult.Value.Average())).TotalMilliseconds,
					TimeSpan.FromTicks(opResult.Value[opResult.Value.Count / 2]).TotalMilliseconds,
					TimeSpan.FromTicks(opResult.Value[0]).TotalMilliseconds,
					TimeSpan.FromTicks(opResult.Value[opResult.Value.Count-1]).TotalMilliseconds));
			}
		}
	}
}