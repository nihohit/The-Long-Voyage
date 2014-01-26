using System;

namespace Base
{
    [Serializable]
    public class UnknownTypeException : Exception
    {
        public UnknownTypeException(object obj) : 
            base("Type {0} wasn't defined.".FormatWith(obj.ToString()))
        { }
    }
}
