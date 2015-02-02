using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base
{
    using System.Reflection;

    #region ConstructorBasedJSONParser

    public static class ObjectConstructor
    {
        #region public methods

        public static T ParseObject<T>(object parsedObject)
        {
            return (T)ParseObject(parsedObject, typeof(T));
        }

        private static object ParseObject(object parsedObject, Type type)
        {
            if (parsedObject == null)
            {
                return null;
            }

            var constructor = type.GetConstructors().First();
            var parameters = new object[constructor.GetParameters().Count()];
            var objectAsDictionary = parsedObject.SafeCast<IDictionary<string, object>>("parsedObject");

            objectAsDictionary = new Dictionary<string, object>(
                objectAsDictionary,
                StringComparer.InvariantCultureIgnoreCase);
            var index = 0;

            foreach (var param in constructor.GetParameters())
            {
                object obj;

                if (!objectAsDictionary.TryGetValue(param.Name, out obj))
                {
                    if (param.IsOptional)
                    {
                        obj = param.DefaultValue;
                    }
                    else
                    {
                        throw new ArgumentException("Parameter {0} is missing in {1}".FormatWith(param.Name, objectAsDictionary));
                    }
                }

                if (param.ParameterType.IsPrimitive)
                {
                    parameters[index] = Convert.ChangeType(obj, param.ParameterType);
                }
                else if (param.ParameterType.IsEnum)
                {
                    parameters[index] = Enum.Parse(param.ParameterType, obj.ToString());
                }
                else if (param.ParameterType == typeof(string))
                {
                    parameters[index] = obj;
                }
                else if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type underlyingType = param.ParameterType.GetGenericArguments()[0];
                    var method = typeof(ObjectConstructor).GetMethod(
                        "ParseEnumerable",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    method = method.MakeGenericMethod(underlyingType);
                    parameters[index] = method.Invoke(null, new[] { obj });
                }
                else
                {
                    parameters[index] = ParseObject(obj, param.ParameterType);
                }

                index++;
            }

            return constructor.Invoke(parameters);
        }

        private static IEnumerable<TValue> ParseEnumerable<TValue>(object enumerable)
        {
            var objectAsEnumerable = enumerable.SafeCast<IEnumerable<object>>("enumerable");

            return objectAsEnumerable.Select(item => ParseObject<TValue>(item));
        }

        #endregion public methods
    }

    #endregion ConstructorBasedJSONParser
}