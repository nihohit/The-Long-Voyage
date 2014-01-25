using UnityEngine;
using System.Collections;

public class MarkerScript : MonoBehaviour {
	public SpriteRenderer renderer;
	
	// Use this for initialization
	void Start () 
	{
		renderer.enabled = false;
	}
	
	public void Mark(Vector3 position)
	{
		renderer.enabled = true;
		renderer.transform.position = position;
	}
	
	public void Unmark()
	{
		renderer.enabled = false;
	}
}
