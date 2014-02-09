﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Timer
{
private static readonly IDictionary<String, IDictionary<String, Stopwatch>> s_timers = new Dictionary<String, IDictionary<String, Stopwatch>>();

public static void StartTiming(string name, string operation)
{       
#if DEBUG
    lock(s_timers)
    {
        IDictionary<String, Stopwatch> dict = null;
        Stopwatch timer = null;
        if(!s_timers.TryGetValue(name, out dict))
        {
            dict = new Dictionary<string, Stopwatch>();
            s_timers.Add(name, dict);
        }
        if (!dict.TryGetValue(operation, out timer))
        {
            timer = new Stopwatch();
            dict.Add(operation, timer);
        }
        timer.Start();
    }
#endif
}

public static void StopTiming(string name, string operation)
{
#if DEBUG
    lock(s_timers)
    {
        s_timers[name][operation].Stop();
    }
#endif
}

public static void PrintTimes()
{
    foreach(var timer in s_timers.Keys)
    {
        foreach(var operation in s_timers[timer].Keys)
        {
			UnityEngine.Debug.Log("{0} time - {1}".FormatWith(operation, s_timers[timer][operation].Elapsed));
        }
    }
}

public static TimeSpan GetTime(string name, string operation)
{
    return s_timers[name][operation].Elapsed;
}
}
