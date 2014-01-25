using UnityEngine;
using System.Collections;

public class HexReactor : MonoBehaviour 
{
	private Hex hex;
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
		Debug.Log( "Highlighting Unit" ); 
		selected.Mark(this.transform.position);
	}

	public void Unselect()
	{
		Debug.Log( "Deselecting Unit" ); 
		selected.Unmark();
	}
}
