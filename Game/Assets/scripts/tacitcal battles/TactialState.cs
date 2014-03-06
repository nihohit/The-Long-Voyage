using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class TacticalState
{
    #region fields

    private static HexReactor s_selectedHex;

    private static LinkedList<Loyalty> s_turnOrder;

    private static LinkedListNode<Loyalty> s_currentTurn;

    private static object s_lock;
	
    #endregion

    #region properties

    public static object Lock { 
        get 
        { 
            if(s_lock == null)
            {
                s_lock = new object();
            }
            return s_lock; 
        } 
    }

    public static HexReactor SelectedHex
	{ 
		get
		{
			return s_selectedHex;
		}
		set
		{
            lock(Lock)
            {
    			if(s_selectedHex != null && s_selectedHex != value)
    			{
    				s_selectedHex.Unselect();
    			}

    			s_selectedHex = value;
    			if(s_selectedHex !=null)
    				s_selectedHex.Select();
            }
		}
	}

    public static Loyalty CurrentTurn { get { return s_currentTurn.Value; } }

    //for each entity and each hex, the available actions
    private static Dictionary<ActiveEntity, IEnumerable<PotentialAction>> s_availableActions;

    #endregion

    #region public methods

    public static void Init(IEnumerable<Loyalty> players) 
    { 
        s_availableActions = new Dictionary<ActiveEntity, IEnumerable<PotentialAction>>();
        SetTurnOrder(players);
    }

    public static void Endturn()
    {
        if(s_currentTurn == null)
        {
            s_currentTurn = s_turnOrder.First;
        }
        s_currentTurn = s_currentTurn.Next;
    }

    //returns null if can't return actions, otherwise returns all available actions
    public static IEnumerable<PotentialAction> ActionCheckOnSelectedHex()
    {
        if(s_selectedHex.MarkedHex.Content == null)
        {
            return null;
        }

        var activeEntity = s_selectedHex.MarkedHex.Content as ActiveEntity;
        if(activeEntity == null || 
           activeEntity.Loyalty != CurrentTurn)
        {
            return null;
        }

        return s_availableActions.TryGetOrAdd(activeEntity, activeEntity.ComputeActions().ToList);
    }

    public static void RecalculateActions(ActiveEntity activeEntity)
    {
        Assert.AssertConditionMet(activeEntity == null ||
                                  activeEntity.Loyalty != CurrentTurn, 
                                  "entity {0} shouldn't recalculating actions".FormatWith(activeEntity));
        IEnumerable<PotentialAction> actions = null;
        if (s_availableActions.TryGetValue(activeEntity, out actions))
        {
            foreach(var action in actions)
            {
                action.Destroy();
            }
        }
        s_availableActions[activeEntity] = activeEntity.ComputeActions().ToList();
    }

    public static void RecalculateAllActions()
    {
        foreach(var actions in s_availableActions.Values)
        {
            foreach(var action in actions)
            {
                action.Destroy();
            }
        }
        s_availableActions.Clear();
        SelectedHex = SelectedHex;
    }

    #endregion

    #region private method

    private static void SetTurnOrder(IEnumerable<Loyalty> players)
    {
        s_turnOrder = new LinkedList<Loyalty>(players);
        s_currentTurn = s_turnOrder.First;
    }

    #endregion
}