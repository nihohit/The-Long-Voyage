namespace Assets.Scripts.Base.JsonParsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    #region ConstructorBasedJSONParser

    public static class ObjectConstructor
    {
        #region public methods

        public static T ParseObject<T>(object parsedObject)
        {
            var type = typeof(T);
            if (type.IsPrimitive)
            {
                parsedObject = Convert.ChangeType(parsedObject, type);
            }
            else if (type.IsEnum)
            {
                parsedObject = Enum.Parse(type, parsedObject.ToString());
            }
            else if (type == typeof(string))
            {
            }
            else
            {
                parsedObject = ParseObject(parsedObject, typeof(T));
            }

            return (T)parsedObject;
        }

        private static object ParseObject(object parsedObject, Type type)
        {
            if (parsedObject == null)
            {
                return null;
            }

            ConstructorInfo constructor;

            var markedConstructors =
                type.GetConstructors().Where(ctor =>
                    ctor.GetCustomAttributes(false).Any(attr =>
                        attr.GetType() == typeof(ChosenConstructorForParsing))).Materialize();

            Assert.EqualOrLesser(markedConstructors.Count(), 1, "More than a single marked constructor");

            if (markedConstructors.None())
            {
                Assert.AreEqual(type.GetConstructors().Count(), 1, "More than a single construcor");
                constructor = type.GetConstructors().First();
            }
            else
            {
                constructor = markedConstructors.First();
            }

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
                    // it's an enumerable, we'll call find the relevant generic version of ParseEnumerable and call it.
                    Type underlyingType = param.ParameterType.GetGenericArguments()[0];
                    var method = typeof(ObjectConstructor).GetMethod(
                        "ParseEnumerable",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    method = method.MakeGenericMethod(underlyingType);
                    parameters[index] = method.Invoke(null, new[] { obj });
                }
                /*
                else if (param.ParameterType.GetInterfaces().Any(i => i == typeof(IIdentifiable<string>)) && obj is string)
                {
                    // TODO - do we need to handle a generic IIdentifiable?
                    // it's a type that implement IIdentifiable
                    var configurationStorageType = typeof(ConfigurationStorage<>);
                    configurationStorageType = configurationStorageType.MakeGenericType(param.ParameterType);
                    var instanceProperty = configurationStorageType.GetProperty(
                        "Instance",
                        BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static);
                    var instance = instanceProperty.GetGetMethod().Invoke(null, null);
                    var getConfigurationMethod = configurationStorageType.GetMethod("GetConfiguration");

                    parameters[index] = getConfigurationMethod.Invoke(instance, new[] { obj });
                }
                 */
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

    #region constrcutor attrbute

    [AttributeUsage(AttributeTargets.Constructor)]
    public class ChosenConstructorForParsing : Attribute
    {
    }

    #endregion constrcutor attrbute
}