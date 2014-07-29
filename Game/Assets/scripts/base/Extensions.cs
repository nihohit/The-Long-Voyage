using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.scripts.Base
{
    public interface IIdentifiable
    {
        string Name { get; }
    }

    /// <summary>
    /// extensions of basic C# objects
    /// </summary>
    public static class MyExtensions
    {
        public static String FormatWith(this string str, params object[] formattingInfo)
        {
            return String.Format(str, formattingInfo);
        }

        //try to get a value out of a dictionary, and if it doesn't exist, create it by a given method
        public static T TryGetOrAdd<T, S>(this IDictionary<S, T> dict, S key, Func<T> itemCreationMethod)
        {
            T result;
            if (!dict.TryGetValue(key, out result))
            {
                result = itemCreationMethod();
                dict.Add(key, result);
            }
            return result;
        }

        //removes from both sets the common elements.
        public static void ExceptOnBoth<T>(this HashSet<T> thisSet, HashSet<T> otherSet)
        {
            thisSet.SymmetricExceptWith(otherSet);
            otherSet.IntersectWith(thisSet);
            thisSet.ExceptWith(otherSet);
        }

        //converts degrees to radians
        public static float DegreesToRadians(this float degrees)
        {
            return (float)Math.PI * degrees / 180;
        }

        #region timing

        public static void StartTiming(this IIdentifiable timer, string operation)
        {
            Timer.StartTiming(timer.Name, operation);
        }

        public static void StopTiming(this IIdentifiable timer, string operation)
        {
            Timer.StopTiming(timer.Name, operation);
        }

        //Time a single action in debug mode
        public static void TimedAction(this IIdentifiable timer, string operation, Action action)
        {
#if DEBUG
            Timer.StartTiming(timer.Name, operation);
#endif
            action();
#if DEBUG
            Timer.StopTiming(timer.Name, operation);
#endif
        }

        #endregion timing

        #region IEnumerable

        //returns an enumerable with all values of an enumerator
        public static IEnumerable<T> GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        //this function ensures that a given enumeration materializes
        public static IEnumerable<T> Materialize<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T>) return enumerable;
            return enumerable.ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> op)
        {
            if (enumerable != null)
            {
                foreach (var val in enumerable)
                {
                    op(val);
                }
            }
        }

        public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> op)
        {
            return !enumerable.Any(op);
        }

        public static bool None<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        public static T ChooseRandomValue<T>(this IEnumerable<T> group)
        {
            return Randomiser.ChooseValue(group);
        }

        public static IEnumerable<T> ChooseRandomValues<T>(this IEnumerable<T> group, int amount)
        {
            return Randomiser.ChooseValues(group, amount);
        }

        #endregion IEnumerable
    }

    /// <summary>
    /// allows classes to have simple hashing, by sending a list of defining factor to the hasher.
    /// Notice that for good hashing, all values must be from immutable fields.
    /// </summary>
    public static class Hasher
    {
        private static int InitialHash = 53; // Prime number
        private static int Multiplier = 29; // Different prime number

        public static int GetHashCode(params object[] values)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = InitialHash;

                if (values != null)
                {
                    foreach (var currentObject in values)
                    {
                        hash = hash * Multiplier
                                + (currentObject != null ? currentObject.GetHashCode() : 0);
                    }
                }

                return hash;
            }
        }
    }
}