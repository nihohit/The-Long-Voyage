using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base
{
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
            var constructor = type.GetConstructors().First();
            var parameters = new object[constructor.GetParameters().Count()];
            var objectAsDictionary = parsedObject as IDictionary<string, object>;
            var index = 0;

            Assert.NotNull(objectAsDictionary, "objectAsDictionary");

            foreach (var param in constructor.GetParameters())
            {
                object obj;

                if (!objectAsDictionary.TryGetValue(param.Name, out obj))
                {
                    if (param.DefaultValue != null)
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
                else
                {
                    parameters[index] = ParseObject(obj, param.ParameterType);
                }

                index++;
            }

            return constructor.Invoke(parameters);
        }

        #endregion public methods
    }

    #endregion ConstructorBasedJSONParser
}