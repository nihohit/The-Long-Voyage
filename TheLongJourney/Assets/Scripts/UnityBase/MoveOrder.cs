using System;
using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    #region MoveOrder

    public class MoveOrder
    {
        #region properties

        public Action ArrivalCallback { get; private set; }

        public Vector3 Point { get; private set; }

        #endregion properties

        #region constructors

        public MoveOrder(Vector3 point, Action callback)
        {
            Point = point;
            ArrivalCallback = callback;
        }

        #endregion constructors
    }

    #endregion MoveOrder
}