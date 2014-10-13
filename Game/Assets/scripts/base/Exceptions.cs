using System;

namespace Assets.Scripts.Base
{
    /// <summary>
    /// Thrown when a switch receives an illegal value
    /// </summary>
    [Serializable]
    public class UnknownValueException : ArgumentException
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

    /// <summary>
    /// Thrown when an area of code which shouldn't be accessed is.
    /// </summary>
    [Serializable]
    public class UnreachableCodeException : Exception
    {
        public UnreachableCodeException(string message = "") :
            base("Unreachable code. {0}".FormatWith(message))
        { }
    }

    /// <summary>
    /// Thrown when looking for a certain value in JSON deserializer
    /// </summary>
    [Serializable]
    public class ValueNotFoundException : ArgumentException
    {
        public ValueNotFoundException(string propertyName, Type type) :
            base("Property {0} not found while deserializing type {1}".FormatWith(propertyName, type.Name))
        { }
    }

    /// <summary>
    /// Thrown when looking for a certain value in JSON deserializer
    /// </summary>
    [Serializable]
    public class WrongValueType : ArgumentException
    {
        public WrongValueType(string propertyName, Type assumedType, Type realType) :
            base("Property {0} was not of type {1}, but {2}".FormatWith(propertyName, assumedType.Name, realType.Name))
        { }
    }
}