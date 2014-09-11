using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base
{
    /// <summary>
    /// Initializes a single Random object for the whole program, in order to overcome flaws in Random implementation.
    /// </summary>
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

        public static double NextDouble(double min, double max)
        {
            return min + s_staticRandom.NextDouble() * (max - min);
        }

        //See if random sample comes lower than the given chance
        public static bool ProbabilityCheck(double chance)
        {
            Assert.EqualOrLesser(chance, 1, "we can't have a probablity higher than 1");
            return (NextDouble() <= chance);
        }

        //choose a single value out of a collection
        public static T ChooseValue<T>(IEnumerable<T> group)
        {
            Assert.NotNullOrEmpty(group, "group");
            return group.ElementAt(Next(group.Count()));
        }

        //choose several values out of a collection
        public static IEnumerable<T> ChooseValues<T>(IEnumerable<T> group, int amount)
        {
            Assert.NotNullOrEmpty(group, "group");
            int totalAmount = group.Count();
            Assert.EqualOrLesser(amount, totalAmount);
            var list = new List<T>();
            foreach (var element in group)
            {
                if (Randomiser.ProbabilityCheck((double)amount / (double)totalAmount))
                {
                    list.Add(element);
                    amount--;
                }
                if (amount == 0) break;
                totalAmount--;
            }
            return list;
        }

        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> group)
        {
            var buffer = group.ToList();

            for (int i = 0; i < buffer.Count; i++)
            {
                int j = s_staticRandom.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}