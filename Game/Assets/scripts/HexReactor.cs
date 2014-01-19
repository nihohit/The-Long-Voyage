using UnityEngine;
using System.Collections;

public class HexReactor : MonoBehaviour 
{
	private Hex hex;
	private Behaviour selected;
	
	void Start( ) 
	{ 
		selected = (Behaviour)GetComponent("Halo");
		selected.enabled = false;
	}
	
	void Update( ) 
	{ 

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
		selected.enabled = true;
	}

	public void Unselect()
	{
		Debug.Log( "Deselecting Unit" ); 
		selected.enabled = false;
	}
}
