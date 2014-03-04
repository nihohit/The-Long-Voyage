using System;

[Serializable]
public class UnknownTypeException : Exception
{
    public UnknownTypeException(object obj) : 
        base("Type {0} wasn't defined.".FormatWith(obj.ToString()))
    { }
}

[Serializable]
public class AssertedException : Exception
{
    public AssertedException(string message) : 
        base("Condition {0} wasn't met".FormatWith(message))
     {}
}
