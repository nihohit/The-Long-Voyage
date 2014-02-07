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
		m_individualMarker.Unmark();
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

	void OnMouseDown () 
	{
		if (Input.GetMouseButton(0)) 
		{
            TacticalState.SelectedHex = this;
		}
	}

	public void Select()
	{
		Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex.Coordinates)); 
		s_selected.Mark(this.transform.position);
	}

	public void Unselect()
	{
		Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex.Coordinates));
		s_selected.Unmark();
	}
}
