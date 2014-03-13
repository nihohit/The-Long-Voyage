using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexReactor : CircularButton 
{
    #region private fields

	public Hex MarkedHex { get; set; }
	private MarkerScript m_movementPathMarker;
    private MarkerScript m_fogOfWarMarker;
    private MarkerScript m_radarBlipMarker;
	private static MarkerScript s_selected;


    #endregion

    #region public methods

    public HexReactor()
    {
        base.Action = () => TacticalState.SelectedHex = this;
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
        if(MarkedHex.Content != null)
        {
            MarkedHex.Content.Marker.Mark();
        }
    }
    
    public void DisplayFogOfWarMarker()
    {
        m_fogOfWarMarker = AddAndDisplayMarker(m_fogOfWarMarker, "FogOfWar");
        if(MarkedHex.Content != null)
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

    #endregion

	public static void Init()
	{
		s_selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
        s_selected.Unmark();
	}

    void Start()
    {
        DisplayFogOfWarMarker();
    }

	public void Select()
	{
		//Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex)); 
		s_selected.Mark(this.transform.position);
        var actions = TacticalState.ActionCheckOnSelectedHex();
        if (actions != null)
        {
            foreach(var action in actions)
            {
                action.DisplayButton();
            }
        }
	}

	public void Unselect()
	{
		//Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex));
		s_selected.Unmark();
        var actions = TacticalState.ActionCheckOnSelectedHex();
        if (actions != null)
        {
            foreach (var action in actions)
            {
                action.RemoveDisplay();
            }
        }
	}

    #endregion

    #region private methods

    private void RemoveMarker(MarkerScript marker)
    {
        if (marker != null)
        {
            marker.Unmark();
        }
    }

    private MarkerScript AddAndDisplayMarker(MarkerScript marker, string markerName)
    {
        if(marker == null)
        {
            marker = ((GameObject)Instantiate(Resources.Load(markerName), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            marker.internalRenderer = marker.GetComponent<SpriteRenderer>();
        }
        marker.Mark(transform.position);
        return marker;
    }

    #endregion
}
