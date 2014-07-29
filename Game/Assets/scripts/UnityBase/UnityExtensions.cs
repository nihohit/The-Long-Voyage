using System;
using UnityEngine;

namespace Assets.scripts.UnityBase
{
    /// <summary>
    /// Extension class for unity engine objects
    /// </summary>
    public static class UnityExtensions
    {
        public static float GetAngleBetweenTwoPoints(this Vector2 from, Vector2 to)
        {
            var differenceVector = to - from;
            var angle = Vector2.Angle(new Vector2(0, 1), differenceVector);
            if (differenceVector.x < 0)
            {
                angle = 360 - angle;
            }
            return angle;
        }

        public static float GetAngleBetweenTwoPoints(this Vector3 from, Vector3 to)
        {
            var from2 = new Vector2(from.x, from.y);
            var to2 = new Vector2(to.x, to.y);
            return from2.GetAngleBetweenTwoPoints(to2);
        }

        public static void DestroyGameObject(this MonoBehaviour unityObject)
        {
            UnityEngine.Object.Destroy(unityObject.gameObject);
        }

        public static double Distance(this Vector3 origin, Vector3 target)
        {
            return Math.Sqrt(Math.Pow(origin.x - target.x, 2) + Math.Pow(origin.y - target.y, 2) + Math.Pow(origin.z - target.z, 2));
        }
    }
}