using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexReactor : MonoBehaviour 
{
	public Hex MarkedHex { get; set; }
	private MarkerScript m_individualMarker;
	private static MarkerScript s_selected;
	private static List<Hex> s_foundPath;
	private static List<Hex> FoundPath { 
		get
		{
			return s_foundPath;
		}
		set 
		{
			if(s_foundPath != null)
			{
				foreach(var hex in s_foundPath)
				{
					hex.Reactor.RemoveIndividualMarker();
				}
			}
			s_foundPath = value;
		}
	}

	public void RemoveIndividualMarker()
	{
		m_individualMarker.Unmark();
	}

	public void DisplayIndividualMarker()
	{
		if(m_individualMarker == null)
		{
			m_individualMarker = ((GameObject)Instantiate(Resources.Load("PathMarker"), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
		}
	}

	public static void Init()
	{
		s_selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
	}

	void OnMouseOver () 
	{
		if (Input.GetMouseButton(0)) 
		{
			if(TacticalState.Instance.SelectedHex == null || this.MarkedHex.HexContent != null)
			{
				FoundPath = null;
				TacticalState.Instance.SelectedHex = this;
			}
			var mover = TacticalState.Instance.SelectedHex.MarkedHex.HexContent as MovingEntity;
			if(mover == null)
			{
				FoundPath = null;
				TacticalState.Instance.SelectedHex = this;
			}
			else
			{
				if(FoundPath == null || !FoundPath.Last().Equals(this.MarkedHex))
				{
					FoundPath = AStar.FindPath(TacticalState.Instance.SelectedHex.MarkedHex, 
					                           this.MarkedHex, 
					                           new AStarConfiguration(
						mover.TraversalMethod,
						(hex) => { return TacticalState.Instance.SelectedHex.MarkedHex.Dist (hex); }));
				}
				else
				{
					FoundPath.Last().HexContent = TacticalState.Instance.SelectedHex.MarkedHex.HexContent;
					FoundPath = null;
				}
			}
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
		MarkedHex.HexContent = new Mech(null, DecisionType.PlayerControlled);
		MarkedHex.HexContent.Marker = ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<MarkerScript>();
		s_selected.Unmark();
	}
}
