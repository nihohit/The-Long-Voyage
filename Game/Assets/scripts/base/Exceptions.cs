using System;

namespace Assets.scripts.Base
{
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
            base("Condition wasn't met : {0}".FormatWith(message))
        { }
    }
}