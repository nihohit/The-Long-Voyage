using UnityEngine;
using System.Collections;
using System.Linq;

public class HexReactor : MonoBehaviour 
{
	public Hex MarkedHex { get; set; }
	private static MarkerScript selected;

	public static void Init()
	{
		selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
	}

	void OnMouseDown()
	{
		TacticalState.Instance.SelectedHex = this;
	}

	void OnMouseOver () 
	{
		if (Input.GetMouseButton(0)) 
		{
			TacticalState.Instance.SelectedHex = this;
		}
	}

	public void Select()
	{
		Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex.Coordinates)); 
		selected.Mark(this.transform.position);
	}

	public void Unselect()
	{
		Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex.Coordinates));
		selected.Unmark();
	}
}
