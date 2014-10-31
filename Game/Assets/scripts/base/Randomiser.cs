﻿using System;
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

        public static double NextDouble(double max)
        {
            return NextDouble(0, max);
        }

        public static double NextDouble(double min, double max)
        {
            return min + s_staticRandom.NextDouble() * (max - min);
        }

        //See if random sample comes lower than the given chance
        public static bool ProbabilityCheck(double chance)
        {
            Assert.EqualOrLesser(chance, 1, "we can't have a probablity higher than 1");
            Assert.EqualOrGreater(chance, 0, "we can't have a probablity lower than 0");
            return (NextDouble() <= chance);
        }

        //choose a single value out of a collection
        public static T ChooseValue<T>(IEnumerable<T> group)
        {
            Assert.NotNullOrEmpty(group, "group");
            T current = default(T);
            int count = 0;
            foreach (T element in group)
            {
                count++;
                if (s_staticRandom.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }

        //choose several values out of a collection
        public static IEnumerable<T> ChooseValues<T>(IEnumerable<T> group, int amount)
        {
            return Shuffle(group).Take(amount);
        }

        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> group)
        {
            Assert.NotNullOrEmpty(group, "group");
            var buffer = group.ToList();

            for (int i = 0; i < buffer.Count; i++)
            {
                int j = s_staticRandom.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }

        internal static bool CoinToss()
        {
            return Next(2) > 0;
        }

        public static IEnumerable<T> ChooseWeightedValues<T>(IDictionary<T, double> dictionary, int amount)
        {
            var chooser = new WeightedValuesChooser<T>();

            return chooser.ChooseWeightedValues(dictionary, amount);
        }

        #region WeightedValuesChooser

        /// This is taken from here https://stackoverflow.com/questions/11775946/select-x-random-elements-from-a-weighted-list-in-c-sharp-without-replacement
        /// and adapted to a generic case.
        /// <typeparam name="T"></typeparam>
        private class WeightedValuesChooser<T>
        {
            private List<Node> GenerateHeap(IDictionary<T, double> dictionary)
            {
                var nodes = new List<Node>();
                nodes.Add(null);

                nodes.AddRange(dictionary.Select(pair => new Node(pair.Value, pair.Key, pair.Value)));

                for (int i = nodes.Count - 1; i > 1; i--)
                {
                    nodes[i >> 1].TotalWeight += nodes[i].TotalWeight;
                }

                return nodes;
            }

            private T PopFromHeap(List<Node> heap)
            {
                var gas = NextDouble(heap[1].TotalWeight);
                int i = 1;

                while (gas >= heap[i].Weight)
                {
                    gas -= heap[i].Weight;
                    i <<= 1;

                    if (gas >= heap[i].TotalWeight)
                    {
                        gas -= heap[i].TotalWeight;
                        i += 1;
                    }
                }

                var weight = heap[i].Weight;
                T card = heap[i].Value;

                heap[i].Weight = 0;

                while (i > 0)
                {
                    heap[i].TotalWeight -= weight;
                    i >>= 1;
                }

                return card;
            }

            public IEnumerable<T> ChooseWeightedValues(IDictionary<T, double> dictionary, int amount)
            {
                var values = new List<T>();

                var nodesHeap = GenerateHeap(dictionary);

                for (int i = 0; i < amount; i++)
                {
                    values.Add(PopFromHeap(nodesHeap));
                }

                return values;
            }

            private class Node
            {
                public double Weight { get; set; }

                public T Value { get; private set; }

                public double TotalWeight { get; set; }

                public Node(double weight, T value, double totalWeight)
                {
                    Weight = weight;
                    Value = value;
                    TotalWeight = totalWeight;
                }
            }
        }

        #endregion WeightedValuesChooser
    }
}