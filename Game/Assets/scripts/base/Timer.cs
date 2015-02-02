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
        private static readonly IDictionary<string, IDictionary<string, Stopwatch>> r_timers = new Dictionary<string, IDictionary<string, Stopwatch>>();

        public static void StartTiming(string name, string operation)
        {
#if DEBUG
            lock (r_timers)
            {
                var dict = r_timers.TryGetOrAdd(name, () => new Dictionary<string, Stopwatch>());
                var timer = dict.TryGetOrAdd(operation, () => new Stopwatch());
                timer.Start();
            }
#endif
        }

        public static void StopTiming(string name, string operation)
        {
#if DEBUG
            lock (r_timers)
            {
                r_timers.Get(name, "Timers dictionary").Get(operation, name).Stop();
            }
#endif
        }

        public static void PrintTimes()
        {
            foreach (var timer in r_timers.Keys)
            {
                foreach (var operation in r_timers[timer].Keys)
                {
                    UnityEngine.Debug.Log("{0} time - {1}".FormatWith(operation, r_timers[timer][operation].Elapsed));
                }
            }
        }

        public static TimeSpan GetTime(string name, string operation)
        {
            return r_timers.Get(name, "Timers dictionary").Get(operation, name).Elapsed;
        }
    }
}