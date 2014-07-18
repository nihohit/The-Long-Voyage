using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.UnityBase
{
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
    }
}
