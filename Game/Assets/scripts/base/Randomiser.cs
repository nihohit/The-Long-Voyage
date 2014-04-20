using System;
using System.Collections.Generic;
using System.Linq;

public static class Randomiser
{
    private static readonly Random s_staticRandom = new Random();

    public static int Next()
    {
        return s_staticRandom.Next();
    }

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

    public static bool ProbabilityCheck(double chance)
    {
        Assert.EqualOrLesser(chance, 1, "we can't have a probablity higher than 1");
        return (NextDouble() <= chance);
    }

    public static T ChooseValue<T>(IEnumerable<T> group)
    {
        Assert.NotNullOrEmpty(group, "group");
        return group.ElementAt(Next(group.Count()));
    }

    public static IEnumerable<T> ChooseValues<T>(IEnumerable<T> group, int amount)
    {
        Assert.NotNullOrEmpty(group, "group");
        int totalAmount = group.Count();
        Assert.EqualOrLesser(amount, totalAmount);
        var list = new List<T>();
        foreach(var element in group)
        {
            if(Randomiser.ProbabilityCheck((double)amount / (double)totalAmount))
            {
                list.Add(element);
                amount--;
            }
            if(amount == 0) break;
            totalAmount--;
        }
        return list;
    }
}

