using System;

namespace Assets.scripts.Base
{
    /// <summary>
    /// Thrown when a switch receives an illegal value
    /// </summary>
    [Serializable]
    public class UnknownValueException : Exception
    {
        public UnknownValueException(object obj) :
            base("Type {0} wasn't defined.".FormatWith(obj.ToString()))
        { }
    }

    /// <summary>
    /// Thrown when an assert is fails
    /// </summary>
    [Serializable]
    public class AssertedException : Exception
    {
        public AssertedException(string message) :
            base("Condition wasn't met : {0}".FormatWith(message))
        { }
    }
}