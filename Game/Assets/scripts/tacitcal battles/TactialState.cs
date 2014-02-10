using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class TacticalState
{
    #region fields

    private static HexReactor m_selectedHex;

    private static LinkedList<Loyalty> m_turnOrder;

    private static LinkedListNode<Loyalty> m_currentTurn;
	
    #endregion

    #region properties

    public static HexReactor SelectedHex
	{ 
		get
		{
			return m_selectedHex;
		}
		set
		{
			if(value == null && m_selectedHex != null)
			{
				m_selectedHex.Unselect();
			}

			m_selectedHex = value;
			if(m_selectedHex !=null)
				m_selectedHex.Select();
		}
	}

    public static Loyalty CurrentTurn { get { return m_currentTurn.Value; } }

    //for each entity and each hex, the available actions
    public static Dictionary<ActiveEntity, IEnumerable<PotentialAction>> AvailableActions { get; private set; }

    #endregion

    #region public methods

    public static void Init(IEnumerable<Loyalty> players) 
    { 
        AvailableActions = new Dictionary<ActiveEntity, IEnumerable<PotentialAction>>();
        SetTurnOrder(players);
    }

    public static void Endturn()
    {
        if(m_currentTurn == null)
        {
            m_currentTurn = m_turnOrder.First;
        }
        m_currentTurn = m_currentTurn.Next;
    }

    //returns null if can't return actions, otherwise returns all available actions
    public static IEnumerable<PotentialAction> ActionCheckOnSelectedHex()
    {
        if(m_selectedHex.MarkedHex.Content == null)
        {
            return null;
        }

        var activeEntity = m_selectedHex.MarkedHex.Content as ActiveEntity;
        if(activeEntity == null || 
           activeEntity.Loyalty != CurrentTurn)
        {
            return null;
        }

        IEnumerable<PotentialAction> actions = null;
        if (!AvailableActions.TryGetValue(activeEntity, out actions))
        {
            actions = activeEntity.ComputeActions();
            AvailableActions.Add(activeEntity, actions);
        }
        return actions;
    }

    public static void RecalculateActions(ActiveEntity activeEntity)
    {
        if (activeEntity == null ||
           activeEntity.Loyalty != CurrentTurn)
        {
            throw new Exception("entity {0} shouldn't recalculating actions".FormatWith(activeEntity));
        }
        IEnumerable<PotentialAction> actions = null;
        if (AvailableActions.TryGetValue(activeEntity, out actions))
        {
            foreach(var action in actions)
            {
                action.Destroy();
            }
        }
        AvailableActions[activeEntity] = activeEntity.ComputeActions();
    }

    #endregion

    #region private method

    private static void SetTurnOrder(IEnumerable<Loyalty> players)
    {
        m_turnOrder = new LinkedList<Loyalty>(players);
        m_currentTurn = m_turnOrder.First;
    }

    #endregion
}