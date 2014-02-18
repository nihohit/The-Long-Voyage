using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexReactor : MonoBehaviour 
{
	public Hex MarkedHex { get; set; }
	private MarkerScript m_individualMarker;
	private static MarkerScript s_selected;

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

	void OnMouseOver () 
	{
		if (Input.GetMouseButton(0)) 
		{
            TacticalState.SelectedHex = this;
		}
        if (Input.GetMouseButton(1))
        {
            if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content == null)
            {
                //TODO - this is just a temporary measure, to create mechs
                var mech = new Mech(null, ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
                mech.Marker.internalRenderer = mech.Marker.GetComponent<SpriteRenderer>();
                TacticalState.SelectedHex.MarkedHex.Content = mech;
            }
            TacticalState.SelectedHex = null;
        }
	}

	public void Select()
	{
		Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex.Coordinates)); 
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
		Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex.Coordinates));
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
