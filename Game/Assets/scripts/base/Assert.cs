
public static class Assert
{
    public static void EqualOrLesser(int num, int top, string additionalMessage)
    {
        AssertConditionMet(num <= top, "{0} is larger than {1} {2}".FormatWith(num, top, additionalMessage));
    }

    public static void EqualOrLesser(double num, double top, string additionalMessage)
    {
        AssertConditionMet(num <= top, "{0} is larger than {1} {2}".FormatWith(num, top, additionalMessage));
    }

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
        AssertConditionMet(a == null, "{0} isn't null {1}".FormatWith(name, additionalMessage));
    }

    public static void NotNull(object a, string name)
    {
        NotNull(a, name, string.Empty);
    }
    
    public static void NotNull(object a, string name, string additionalMessage)
    {
        AssertConditionMet(a != null, "{0} is null {1}".FormatWith(name, additionalMessage));
    }

    public static void AreEqual<T>(T a, T b)
    {
        AreEqual(a,b,string.Empty);
    }

    public static void AreEqual<T>(T a, T b, string additionalMessage)
    {
        AssertConditionMet(a.Equals(b), "{0} isn't equal to {1} {2}".FormatWith(a,b,additionalMessage));
    }

    public static void AssertConditionMet(bool condition, string message)
    {
        if(!condition)
        {
            throw new AssertedException(message);
        }
    }
}


