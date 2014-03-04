using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexReactor : CircularButton 
{
	public Hex MarkedHex { get; set; }
	private MarkerScript m_individualMarker;
	private static MarkerScript s_selected;

    public HexReactor()
    {
        base.Action = () => TacticalState.SelectedHex = this;
    }

	public void RemoveIndividualMarker()
	{
        if (m_individualMarker != null)
        {
            m_individualMarker.Unmark();
        }
	}

	public void DisplayIndividualMarker()
	{
		if(m_individualMarker == null)
		{
			m_individualMarker = ((GameObject)Instantiate(Resources.Load("PathMarker"), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            m_individualMarker.internalRenderer = m_individualMarker.GetComponent<SpriteRenderer>();
		}
        m_individualMarker.Mark(transform.position);
	}

	public static void Init()
	{
		s_selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
        s_selected.Unmark();
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
}
