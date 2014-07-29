using System.Collections.Generic;
using System.Linq;

namespace Assets.scripts.Base
{
    /// <summary>
    /// These are all the code sanity checks we use in order to assert the internal state of the code
    /// </summary>
    public static class Assert
    {
        public static void NotEqual(int first, int second, string additionalMessage)
        {
            AssertConditionMet(first != second, additionalMessage);
        }

        public static void NotEqual(int first, int second)
        {
            NotEqual(first, second, string.Empty);
        }

        public static void EqualOrLesser(int num, int top, string additionalMessage)
        {
            AssertConditionMet(num <= top, "{0} is larger than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        public static void EqualOrLesser(int num, int top)
        {
            EqualOrLesser(num, top, string.Empty);
        }

        public static void EqualOrLesser(double num, double top)
        {
            EqualOrLesser(num, top, string.Empty);
        }

        public static void EqualOrLesser(double num, double top, string additionalMessage)
        {
            AssertConditionMet(num <= top, "{0} is larger than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        public static void Lesser(double num, double top)
        {
            Lesser(num, top, string.Empty);
        }

        public static void Lesser(double num, double top, string additionalMessage)
        {
            AssertConditionMet(num < top, "{0} is larger than {1}. {2}".FormatWith(num, top, additionalMessage));
        }

        //to be put where a correct run shouldn't reach
        public static void UnreachableCode(string message)
        {
            AssertConditionMet(false, "unreachable code: {0}".FormatWith(message));
        }

        public static void UnreachableCode()
        {
            UnreachableCode(string.Empty);
        }

        public static void IsNull(object a, string name)
        {
            IsNull(a, name, string.Empty);
        }

        public static void IsNull(object a, string name, string additionalMessage)
        {
            AssertConditionMet(a == null, "{0} isn't null. {1}".FormatWith(name, additionalMessage));
        }

        public static void NotNull(object a, string name)
        {
            NotNull(a, name, string.Empty);
        }

        public static void NotNull(object a, string name, string additionalMessage)
        {
            AssertConditionMet(a != null, "{0} is null. {1}".FormatWith(name, additionalMessage));
        }

        public static void NotNullOrEmpty<T>(IEnumerable<T> a, string name)
        {
            NotNullOrEmpty(a, name, string.Empty);
        }

        public static void NotNullOrEmpty<T>(IEnumerable<T> a, string name, string additionalMessage)
        {
            AssertConditionMet(a != null || !a.Any(), "{0} is null or empty. {1}".FormatWith(name, additionalMessage));
        }

        public static void StringNotNullOrEmpty(string a, string name)
        {
            StringNotNullOrEmpty(a, name, string.Empty);
        }

        public static void StringNotNullOrEmpty(string a, string name, string additionalMessage)
        {
            AssertConditionMet(a != null || !a.Equals(string.Empty), "{0} is null or empty. {1}".FormatWith(name, additionalMessage));
        }

        public static void AreEqual<T>(T a, T b)
        {
            AreEqual(a, b, string.Empty);
        }

        public static void AreEqual<T>(T a, T b, string additionalMessage)
        {
            AssertConditionMet(a.Equals(b), "{0} isn't equal to {1}. {2}".FormatWith(a, b, additionalMessage));
        }

        //the core assert check
        public static void AssertConditionMet(bool condition, string message)
        {
            if (!condition)
            {
                throw new AssertedException(message);
            }
        }
    }
}