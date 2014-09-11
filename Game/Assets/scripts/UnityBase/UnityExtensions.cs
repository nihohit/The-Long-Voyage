using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UnityBase
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

        public static double Distance(this Vector3 point, Vector3 otherPoint)
        {
            return Vector3.Distance(point, otherPoint);
        }

        public static float Distance(this Vector2 point, Vector2 otherPoint)
        {
            return Vector2.Distance(point, otherPoint);
        }

        // return the bounds of a collider
        public static Rect Bounds(this BoxCollider2D collider)
        {
            var size = collider.size;
            var sizeX = size.x / 2;
            var sizeY = size.y / 2;
            var startingPoint = (Vector2)collider.transform.position + new Vector2(-sizeX, -sizeY);
            return new Rect(startingPoint.x, startingPoint.y, size.x, size.y);
        }

        // find the coordinates in an array of a certain item
        public static Vector2 GetCoordinates<T>(this T[,] array, T searchedItem)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j].Equals(searchedItem))
                    {
                        return new Vector2(i, j);
                    }
                }
            }
            throw new Exception("item not found");
        }

        // Create the Wait function as an enumerator
        public static IEnumerator Wait(this object obj, float time)
        {
            yield return new WaitForSeconds(time);
        }

        // divide a measure of time between different items, with a small addition per item.
        public static float TimePerItem<T>(this IEnumerable<T> collection, float baseTime, float minimum)
        {
            return baseTime.TimePerAmount(collection.Count(), minimum);
        }

        // divide a measure of time between different items, with a small addition per item.
        public static float TimePerAmount(this float baseTime, int amountOfItems, float minimum)
        {
            return Mathf.Max(baseTime / amountOfItems, minimum);
        }
    }
}