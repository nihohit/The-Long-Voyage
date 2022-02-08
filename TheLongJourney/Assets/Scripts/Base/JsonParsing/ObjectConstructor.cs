namespace Assets.Scripts.Base.JsonParsing {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  using UnityEngine;

  #region ConstructorBasedJSONParser

  public static class ObjectConstructor {
    #region public methods

    public static T ParseObject<T>(object parsedObject) {
      var type = typeof(T);
      if (type.IsPrimitive) {
        parsedObject = Convert.ChangeType(parsedObject, type);
      } else if (type.IsEnum) {
        parsedObject = Enum.Parse(type, parsedObject.ToString());
      } else if (type == typeof(string)) {
      } else {
        try {
          parsedObject = ParseObject(parsedObject, type);
        } catch (Exception ex) {
          throw new Exception("Failed parsing:{0}".FormatWith(parsedObject), ex);
        }
      }

      return (T)parsedObject;
    }

    private static object ParseObject(object parsedObject, Type type) {
      if (parsedObject == null) {
        return null;
      }

      ConstructorInfo constructor;

      var markedConstructors =
        type.GetConstructors().Where(ctor =>
          ctor.GetCustomAttributes(false).Any(attr =>
            attr.GetType() == typeof(ChosenConstructorForParsing))).ToList();

      Assert.EqualOrLesser(markedConstructors.Count(), 1, "More than a single marked constructor");

      if (markedConstructors.None()) {
        Assert.AreEqual(type.GetConstructors().Count(), 1, "More than a single constructor");
        constructor = type.GetConstructors().First();
      } else {
        constructor = markedConstructors.First();
      }

      var parameters = new List<object>();
      var objectAsDictionary = parsedObject.SafeCast<IDictionary<string, object>>("parsedObject");

      objectAsDictionary = new Dictionary<string, object>(
        objectAsDictionary,
        StringComparer.InvariantCultureIgnoreCase);

      foreach (var param in constructor.GetParameters()) {
        try {
          parameters.Add(parseParameter(param, objectAsDictionary));
        } catch (AssertedException ex) {
          throw new AssertedException(" for parameter {0} in type {1}".FormatWith(param.Name, type.Name), ex);
        } catch (TargetInvocationException ex) {
          throw new AssertedException(" for parameter {0} in type {1}".FormatWith(param.Name, type.Name), (AssertedException)ex.InnerException);
        }
      }

      return constructor.Invoke(parameters.ToArray());
    }

    private static object parseParameter(ParameterInfo param, IDictionary<string, object> objectAsDictionary) {
      object obj;

      if (!objectAsDictionary.TryGetValue(param.Name, out obj)) {
        Assert.AssertConditionMet(param.IsOptional, "Missing parameter isn't optional");
        obj = param.DefaultValue;
      }

      if (param.ParameterType.IsPrimitive) {
        return Convert.ChangeType(obj, param.ParameterType);
      } else if (param.ParameterType.IsEnum) {
        return Enum.Parse(param.ParameterType, obj.ToString());
      } else if (param.ParameterType == typeof(string)) {
        return obj;
      } else if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
        // it's an enumerable, we'll call find the relevant generic version of ParseEnumerable and call it.
        Type underlyingType = param.ParameterType.GetGenericArguments()[0];
        var method = typeof(ObjectConstructor).GetMethod(
          "ParseEnumerable",
          BindingFlags.NonPublic | BindingFlags.Static);
        method = method.MakeGenericMethod(underlyingType);
        return method.Invoke(null, new[] { obj });
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
        else {
        return ParseObject(obj, param.ParameterType);
      }
    }

    private static IEnumerable<TValue> ParseEnumerable<TValue>(object enumerable) {
      if (enumerable == null) {
        return Enumerable.Empty<TValue>();
      }

      var objectAsEnumerable = enumerable.SafeCast<IEnumerable<object>>("enumerable");

      return objectAsEnumerable.Select(item => ParseObject<TValue>(item));
    }

    #endregion public methods
  }

  #endregion ConstructorBasedJSONParser

  #region constrcutor attribute

  [AttributeUsage(AttributeTargets.Constructor)]
  public class ChosenConstructorForParsing : Attribute {
  }

  #endregion constrcutor attrbute
}