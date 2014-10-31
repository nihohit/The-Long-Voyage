using System;
using System.Collections.Generic;
using Assets.Scripts.LogicBase;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene.PathFinding
{
    #region delegates

    internal delegate List<HexReactor> PathfindFunction(Vector2 entry, Vector2 goal, MovementType traversalMethod);

    internal delegate float Heuristic(HexReactor check);

    #endregion delegates

    #region AstarNode

    /// <summary>
    /// A node in the emergent AStar graph, with different value properties
    /// </summary>
    internal class AstarNode : IComparable<AstarNode>
    {
        private readonly int m_gTotalValue; //the value of the size-portion, for clearance
        private float m_gValue;

        #region constructors

        public AstarNode(HexReactor value, float costToEnterThisNode, int gTotal, float heuristicCost, AstarNode parent)
        {
            ChosenHex = value;
            m_gTotalValue = gTotal;
            GValue = costToEnterThisNode;
            FValue = GValue + heuristicCost + gTotal;
            HValue = heuristicCost;
            Parent = parent;
            Open = true;
        }

        public AstarNode(HexReactor value, AstarNode parent) :
            this(value, 0, 0, 0, parent)
        { }

        //only first node uses this
        public AstarNode(HexReactor value, float h) :
            this(value, 0, 0, h, null)
        { }

        #endregion constructors

        #region properties

        public AstarNode Parent { get; set; }

        public HexReactor ChosenHex { get; private set; }

        public bool Open { get; set; }

        public float HValue { get; private set; }

        public float GValue
        {
            get { return m_gValue; }
            set { m_gValue = value; FValue = GValue + HValue + m_gTotalValue; }
        }

        public float FValue { get; private set; }

        #endregion properties

        #region comparers

        int IComparable<AstarNode>.CompareTo(AstarNode other)
        {
            if (FValue > other.FValue) return 1;
            if (FValue < other.FValue) return -1;
            if (HValue > other.HValue) return 1;
            if (HValue < other.HValue) return -1;
            return 0;
        }

        #endregion comparers
    }

    #endregion AstarNode
}