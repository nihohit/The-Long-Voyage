using System;
using System.Collections.Generic;


public interface IIdentifiable
{
	string Name {get;}
}

public static class MyExtensions
{
    public static String FormatWith(this string str, params object[] formattingInfo)
    {
        return String.Format(str, formattingInfo);
    }

    public static IEnumerable<T> GetValues<T>() 
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
        return new HashSet<T>(source);
    }

    public static void StartTiming(this IIdentifiable timer, string operation)
    {
        Timer.StartTiming(timer.Name, operation);
    }

    public static void StopTiming(this IIdentifiable timer, string operation)
    {
        Timer.StopTiming(timer.Name, operation);
    }

    public static void TimeAction(this IIdentifiable timer, string operation, Action action)
    {
#if DEBUG
        Timer.StartTiming(timer.Name, operation);
#endif
        action();
#if DEBUG
        Timer.StopTiming(timer.Name, operation);
#endif
    }

    public static T ChooseRandomValue<T>(this IEnumerable<T> group)
    {
        return Randomiser.ChooseValue(group);
    }
}
