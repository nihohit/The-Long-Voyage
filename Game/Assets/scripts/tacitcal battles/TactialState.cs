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

    private static IDictionary<Loyalty, ActiveEntity> s_controlledEntities;

    private static IDictionary<Entity, IEnumerable<Hex>> s_whatEntitiesSee;

    private static IDictionary<Entity, IEnumerable<Hex>> s_whatEntitiesSeeInRadar;
	
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

        return s_availableActions.TryGetOrAdd(activeEntity, () => activeEntity.ComputeActions());
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

    public void SetSeenHexes(ActiveEntity ent)
    {
        var whatTheEntitySeesNow = ent.FindSeenHexes();
        var whatTheEntitySeesNowInRadar = ent.FindRadarHexes().Except(whatTheEntitySeesNow);
		
        ISet<Hex> pastSeenHexes;
        if(s_whatEntitiesSee.TryGetValue(ent, out pastSeenHexes))
        {
			var whatTheEntitySeesNowSet = new HashSet(whatTheEntitySeesNow);
			var whatTheEntitySeesNowInRadarSet = new HashSet(whatTheEntitySeesNowInRadar);
		
            var pastSeenInRadarHexes = s_whatEntitiesSeeInRadar[ent];
            //this leaves in each list the hexes not in the other
            whatTheEntitySeesNowSet.SymmetricExceptWith(pastSeenHexes);
            whatTheEntitySeesNowInRadarSet.SymmetricExceptWith(pastSeenInRadarHexes);
			
			foreach(var hex in whatTheEntitySeesNowSet)
			{
				hex.Seen();
			}
			foreach(var hex in whatTheEntitySeesNowInRadarSet)
			{
				hex.Detected();
			}
			foreach(var hex in pastSeenHexes)
			{
				hex.Unseen();
			}
			foreach(var hex in pastSeenInRadarHexes)
			{
				hex.Undetected();
			}
        }
		else
		{
			foreach(var hex in whatTheEntitySeesNow)
			{
				hex.Seen();
			}
			foreach(var hex in whatTheEntitySeesNowInRadar)
			{
				hex.Detected();
			}
		}
		s_whatEntitiesSee[ent] = whatTheEntitySeesNow;
		s_whatEntitiesSeeInRadar[ent] = whatTheEntitySeesNowInRadar;
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