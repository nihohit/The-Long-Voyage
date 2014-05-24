using System.Collections.Generic;
using UnityEngine;

public class HexReactor : CircularButton
{
    #region private fields

    public Hex MarkedHex { get; set; }

    private MarkerScript m_movementPathMarker;
    private MarkerScript m_fogOfWarMarker;
    private MarkerScript m_radarBlipMarker;
    private static MarkerScript s_selected;

    #endregion private fields

    #region public methods

    public HexReactor()
    {
        base.Action = CheckIfClickIsOnUI ( () => TacticalState.SelectedHex = this);
    }

    #region markers

    public void RemoveMovementMarker()
    {
        RemoveMarker(m_movementPathMarker);
    }

    public void DisplayMovementMarker()
    {
        m_movementPathMarker = AddAndDisplayMarker(m_movementPathMarker, "PathMarker");
    }

    public void RemoveFogOfWarMarker()
    {
        RemoveMarker(m_fogOfWarMarker);
        RemoveRadarBlipMarker();
        if (MarkedHex.Content != null)
        {
            MarkedHex.Content.Marker.Mark();
        }
    }

    public void DisplayFogOfWarMarker()
    {
        m_fogOfWarMarker = AddAndDisplayMarker(m_fogOfWarMarker, "FogOfWar");
        if (MarkedHex.Content != null)
        {
            MarkedHex.Content.Marker.Unmark();
        }
    }

    public void RemoveRadarBlipMarker()
    {
        RemoveMarker(m_radarBlipMarker);
    }

    public void DisplayRadarBlipMarker()
    {
        m_radarBlipMarker = AddAndDisplayMarker(m_radarBlipMarker, "RadarBlip");
    }

    #endregion markers

    public static void Init()
    {
        s_selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
        s_selected.Unmark();
    }

    private void Start()
    {
        DisplayFogOfWarMarker();
    }

    public void Select()
    {
        //Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex));
        s_selected.Mark(this.transform.position);
        ActionCheck().ForEach(action => action.DisplayButton());
    }

    public void Unselect()
    {
        //Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex));
        s_selected.Unmark();
        ActionCheck().ForEach(action => action.RemoveDisplay());
    }

    #endregion public methods

    #region private methods

    //returns null if can't return actions, otherwise returns all available actions
    private IEnumerable<PotentialAction> ActionCheck()
    {
        var activeEntity = MarkedHex.Content as ActiveEntity;
        if (activeEntity == null || activeEntity.Loyalty != TacticalState.CurrentTurn)
        {
            return null;
        }

        return activeEntity.Actions.Materialize();
    }

    private void RemoveMarker(MarkerScript marker)
    {
        if (marker != null)
        {
            marker.Unmark();
        }
    }

    private MarkerScript AddAndDisplayMarker(MarkerScript marker, string markerName)
    {
        if (marker == null)
        {
            marker = ((GameObject)Instantiate(Resources.Load(markerName), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            marker.internalRenderer = marker.GetComponent<SpriteRenderer>();
        }
        marker.Mark(transform.position);
        return marker;
    }

    #endregion private methods
}