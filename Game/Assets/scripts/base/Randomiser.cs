using System;
using System.Collections.Generic;
using System.Linq;

public static class Randomiser
{
private static readonly Random s_staticRandom = new Random();

public static int Next(int maxValue)
{
    return s_staticRandom.Next(maxValue);
}

public static int Next(int minValue, int maxValue)
{
    return s_staticRandom.Next(minValue, maxValue);
}

public static double NextDouble()
{
    return s_staticRandom.NextDouble();
}

public static T ChooseValue<T>(IEnumerable<T> group)
{
    return group.ElementAt(Next(group.Count()));
}
}

