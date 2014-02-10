using UnityEngine;
using System.Collections;

public class MarkerScript : MonoBehaviour 
{
	public SpriteRenderer internalRenderer;
	
	public void Mark(Vector3 position)
	{
        this.enabled = true;
		internalRenderer.enabled = true;
		internalRenderer.transform.position = position;
	}

    public void Mark()
    {
        Mark(this.transform.position);
    }
	
	public void Unmark()
	{
		internalRenderer.enabled = false;
        this.enabled = false;
	}
}
