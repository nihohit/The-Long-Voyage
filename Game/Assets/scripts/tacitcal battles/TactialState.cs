using UnityEngine;
using System.Collections.Generic;
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

    private static IEnumerable<Hex> s_hexes;

    private static EntityTextureHandler m_textureChanger;
	
    #endregion

    #region properties

    public static bool BattleStarted { get; set; }

    public static HexReactor SelectedHex
	{ 
		get
		{
			return s_selectedHex;
		}
		set
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

    public static Loyalty CurrentTurn { get { return s_currentTurn.Value; } }

    #endregion

    #region public methods

    public static void DestroyActiveEntity(ActiveEntity ent)
    {
        s_activeEntities.Remove(ent);
        //TODO - end battle logic
        if(ent.Loyalty == Loyalty.Player)
        {
            //check if player lost
            if(s_activeEntities.None(entity => entity.Loyalty == Loyalty.Player))
            {
                Debug.Log("Player lost");
                Application.LoadLevel("MainScreen");
            }
        }
        if(ent.Loyalty != Loyalty.Player)
        {
            //check if player won
            if(s_activeEntities.None(entity => entity.Loyalty != Loyalty.Player))
            {
                Debug.Log("Player won");
                Application.LoadLevel("MainScreen");
            }
        }
    }

    public static void Init(IEnumerable<ActiveEntity> entities, IEnumerable<Hex> hexes) 
    { 
        m_textureChanger = new EntityTextureHandler();
        BattleStarted = false;
        s_activeEntities = new HashSet<ActiveEntity>(entities);
        entities.ForEach(ent => m_textureChanger.UpdateEntityTexture(ent));
        SetTurnOrder(entities.Select(ent => ent.Loyalty).Distinct());
        s_hexes = hexes;
    }

    public static void StartTurn()
    {
        s_currentTurn = s_currentTurn.Next;
        if(s_currentTurn == null)
        {
            s_currentTurn = s_turnOrder.First;
        }
        s_hexes.ForEach(hex => hex.ResetSight());
        Debug.Log("Starting {0}'s turn.".FormatWith(CurrentTurn));
        s_activeEntities.Where(ent => ent.Loyalty == CurrentTurn).ForEach(ent => ent.StartTurn());
        SelectedHex = null;
    }

    public static void ResetAllActions()
    {
        s_activeEntities.ForEach(ent => ent.ResetActions());
        SelectedHex = SelectedHex;
    }

    //TODO - remove once we don't have active creation of entities
    public static void AddEntity(Entity ent)
    {
        var active = ent as ActiveEntity;
        if(active != null)
        {
            s_activeEntities.Add(active);
            m_textureChanger.UpdateEntityTexture(ent);
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