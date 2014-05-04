using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class TacticalState
{
    #region fields

    private static HexReactor s_selectedHex;

    private static LinkedList<Loyalty> s_turnOrder;

    private static LinkedListNode<Loyalty> s_currentTurn;

    private static object s_lock;

    //for each entity and each hex, the available actions
    private static HashSet<ActiveEntity> s_activeEntities;
	
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
    			if(s_selectedHex != null)
    			{
    				s_selectedHex.Unselect();
    			}

    			s_selectedHex = value;
    			if(s_selectedHex != null)
    				s_selectedHex.Select();
            }
		}
	}

    public static Loyalty CurrentTurn { get { return s_currentTurn.Value; } }

    #endregion

    #region public methods

    public static void Init(IEnumerable<Loyalty> players) 
    { 
        s_activeEntities = new HashSet<ActiveEntity>();
        SetTurnOrder(players);
    }

    public static void Endturn()
    {
        Debug.Log("ending turn");
        s_currentTurn = s_currentTurn.Next;
        if(s_currentTurn == null)
        {
            s_currentTurn = s_turnOrder.First;
        }
        s_activeEntities.Where(ent => ent.Loyalty == CurrentTurn).ForEach(ent => ent.StartTurn());
        SelectedHex = null;
    }

    public static void ResetAllActions()
    {
        s_activeEntities.ForEach(ent => ent.ResetActions());
        SelectedHex = SelectedHex;
    }

    public static void AddEntity(Entity ent)
    {
        var active = ent as ActiveEntity;
        if(active != null)
        {
            s_activeEntities.Add(active);
        }
        //To refresh the potential actions appearing on screen.
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