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

    private static object m_lock;
	
    #endregion

    #region properties

    public static object Lock { 
        get 
        { 
            if(m_lock == null)
            {
                m_lock = new object();
            }
            return m_lock; 
        } 
    }

    public static HexReactor SelectedHex
	{ 
		get
		{
			return m_selectedHex;
		}
		set
		{
			if(m_selectedHex != null && m_selectedHex != value)
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
    private static Dictionary<ActiveEntity, IEnumerable<PotentialAction>> m_availableActions;

    #endregion

    #region public methods

    public static void Init(IEnumerable<Loyalty> players) 
    { 
        m_availableActions = new Dictionary<ActiveEntity, IEnumerable<PotentialAction>>();
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
        if (!m_availableActions.TryGetValue(activeEntity, out actions))
        {
            actions = activeEntity.ComputeActions();
            m_availableActions.Add(activeEntity, actions);
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
        if (m_availableActions.TryGetValue(activeEntity, out actions))
        {
            foreach(var action in actions)
            {
                action.Destroy();
            }
        }
        m_availableActions[activeEntity] = activeEntity.ComputeActions();
    }

    public static void RecalculateAllActions()
    {
        m_availableActions.Clear();
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