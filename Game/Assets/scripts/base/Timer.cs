using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assets.Scripts.Base
{
    /// <summary>
    /// A static class that holds all the timers running throughout the program.
    /// </summary>
    public static class Timer
    {
        private static readonly IDictionary<String, IDictionary<String, Stopwatch>> s_timers = new Dictionary<String, IDictionary<String, Stopwatch>>();

        public static void StartTiming(string name, string operation)
        {
#if DEBUG
            lock (s_timers)
            {
                var dict = s_timers.TryGetOrAdd(name, () => new Dictionary<string, Stopwatch>());
                var timer = dict.TryGetOrAdd(operation, () => new Stopwatch());
                timer.Start();
            }
#endif
        }

        public static void StopTiming(string name, string operation)
        {
#if DEBUG
            lock (s_timers)
            {
                s_timers.Get(name, "Timers dictionary").Get(operation, name).Stop();
            }
#endif
        }

        public static void PrintTimes()
        {
            foreach (var timer in s_timers.Keys)
            {
                foreach (var operation in s_timers[timer].Keys)
                {
                    UnityEngine.Debug.Log("{0} time - {1}".FormatWith(operation, s_timers[timer][operation].Elapsed));
                }
            }
        }

        public static TimeSpan GetTime(string name, string operation)
        {
            return s_timers.Get(name, "Timers dictionary").Get(operation, name).Elapsed;
        }
    }
}