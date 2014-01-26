using UnityEngine;
using System.Collections;

public class MarkerScript : MonoBehaviour {
	public SpriteRenderer internalRenderer;
	
	// Use this for initialization
	void Start () 
	{
		internalRenderer.enabled = false;
	}
	
	public void Mark(Vector3 position)
	{
		internalRenderer.enabled = true;
		internalRenderer.transform.position = position;
	}
	
	public void Unmark()
	{
		internalRenderer.enabled = false;
	}
}
