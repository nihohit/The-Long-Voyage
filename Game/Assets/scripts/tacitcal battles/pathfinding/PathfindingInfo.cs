using System;
using System.Collections.Generic;
using UnityEngine;

#region delegates

delegate List<Hex> pathfindFunction(Vector2 entry, Vector2 goal, MovementType traversalMethod);
internal delegate float Heuristic(Hex check);
#endregion

#region AstarNode

internal class AstarNode : IComparable<AstarNode>
{
private readonly int m_gTotalValue; //the value of the size-portion, for clearance
private float m_gValue;

#region constructors

public AstarNode(Hex value, float g, int g_total, float h, AstarNode parent)
{
    ChosenHex = value;
    m_gTotalValue = g_total;
    GValue = g;
    FValue = GValue + h + g_total;
    HValue = h;
    Parent = parent;
    Open = true;
}

public AstarNode(Hex value, AstarNode parent)
{
    ChosenHex = value;
    Parent = parent;
    Open = true;
}

//only first node uses this
public AstarNode(Hex value, float h)
{
    ChosenHex = value;
    m_gTotalValue = 0;
    m_gValue = 0;
    FValue = GValue + h;
    HValue = h;
    Parent = null;
    Open = true;
}

#endregion

#region properties

public AstarNode Parent { get; set; }

public Hex ChosenHex { get; private set; }

public bool Open { get; set; }

public float HValue { get; private set; }

public float GValue
{
    get { return m_gValue; }
    set { m_gValue = value; FValue = GValue + HValue + m_gTotalValue; }
}

public float FValue { get; private set; }

#endregion

#region comparers

int IComparable<AstarNode>.CompareTo(AstarNode other)
{
    if (FValue > other.FValue) return 1;
    if (FValue < other.FValue) return -1;
    if (HValue > other.HValue) return 1;
    if (HValue < other.HValue) return -1;
    return 0;
}

static int NodeComparer(AstarNode a, AstarNode b)
{
    if (a.FValue > b.FValue) return 1;
    if (a.FValue < b.FValue) return -1;
    if (a.HValue > b.HValue) return 1;
    if (a.HValue < b.HValue) return -1;
    return 0;
}

#endregion
}

#endregion
