using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base
{
    /// <summary>
    /// These are all the code sanity checks we use in order to assert the internal state of the code
    /// </summary>
    public static class Assert
    {
        public static void NotEqual(object first, object second, string additionalMessage = "")
        {
            AssertConditionMet(!first.Equals(second), "{0} equals {1}. {2}".FormatWith(first, second, additionalMessage));
        }

        public static void EqualOrGreater(double num, double top, string additionalMessage = "")
        {
            AssertConditionMet(num >= top, "{0} is smaller than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        public static void Greater(double num, double top, string additionalMessage = "")
        {
            AssertConditionMet(num > top, "{0} is smaller than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        public static void EqualOrLesser(double num, double top, string additionalMessage = "")
        {
            AssertConditionMet(num <= top, "{0} is larger than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        public static void Lesser(double num, double top, string additionalMessage = "")
        {
            AssertConditionMet(num < top, "{0} is larger than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        //to be put where a correct run shouldn't reach
        public static void UnreachableCode(string message = "")
        {
            AssertConditionMet(false, "unreachable code: {0}".FormatWith(message));
        }

        public static void IsNull(object a, string name, string additionalMessage = "")
        {
            AssertConditionMet(a == null, "\'{0}\' isn't null. {1}".FormatWith(name, additionalMessage));
        }

        public static void NotNull(object a, string name, string additionalMessage = "")
        {
            AssertConditionMet(a != null, "\'{0}\' is null. {1}".FormatWith(name, additionalMessage));
        }

        public static void NotNullOrEmpty<T>(IEnumerable<T> a, string name, string additionalMessage = "")
        {
            AssertConditionMet(a != null && !a.Any(), "\'{0}\' is null or empty. {1}".FormatWith(name, additionalMessage));
        }

        public static void StringNotNullOrEmpty(string str, string variableName, string additionalMessage = "")
        {
            AssertConditionMet(!String.IsNullOrEmpty(str), "\'{0}\' is null or empty. {1}".FormatWith(variableName, additionalMessage));
        }

        public static void AreEqual(object a, object b, string additionalMessage = "")
        {
            AssertConditionMet(a.Equals(b), "{0} isn't equal to {1}. {2}".FormatWith(a, b, additionalMessage));
        }

        //the core assert check
        public static void AssertConditionMet(bool condition, string message, int stackTraceDepth = 1)
        {
            if (!condition)
            {
                throw new AssertedException(message);
            }
        }

        internal static void NotNullOrEmpty(string name, string variableName, string additionalMessage = "")
        {
            AssertConditionMet(!String.IsNullOrEmpty(name), "{0} was null or empty. {1}".FormatWith(variableName, additionalMessage));
        }

        internal static void IsEmpty<T>(IEnumerable<T> list, string variableName, string additionalMessage = "")
        {
            AssertConditionMet(list.None(), "{0} wasn't empty. {1}".FormatWith(variableName, additionalMessage));
        }
    }
}