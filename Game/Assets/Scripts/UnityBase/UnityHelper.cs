using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    public static class UnityHelper
    {
        private readonly static Vector3 r_defaultLocation = Vector3.zero;

        public static T Instantiate<T>() where T : MonoBehaviour
        {
            return Instantiate<T>(r_defaultLocation, GetResourceName(typeof(T)));
        }

        public static T Instantiate<T>(string resourceName) where T : MonoBehaviour
        {
            return Instantiate<T>(r_defaultLocation, resourceName);
        }

        public static T Instantiate<T>(Vector3 location) where T : MonoBehaviour
        {
            return Instantiate<T>(location, GetResourceName(typeof(T)));
        }

        public static T Instantiate<T>(Vector3 location, string resourceName) where T : MonoBehaviour
        {
            return ((GameObject)GameObject.Instantiate(Resources.Load(resourceName), location, Quaternion.identity)).GetComponent<T>();
        }

        private static string GetResourceName(Type t)
        {
            return t.Name.Replace("Script", string.Empty);
        }
    }
}
