using UnityEngine;
using System.Collections;

public class Shot : MonoBehaviour 
{
    private bool m_started = false;
    private Vector2 m_movementFraction;
    private Vector2 m_endPoint;

    public void Init(Vector2 to, Vector2 from)
    {
        transform.position = from;
        m_endPoint = to;
        m_started = true;
        var differenceVector = to - from;
        this.gameObject.transform.Rotate(new Vector3(0,0,360-Vector2.Angle(to, from)));
        m_movementFraction = differenceVector / 30;
    }

	// Use this for initialization
	void Start () 
    {	
        Destroy(this.gameObject,0.5f);
    }
	
	// Update is called once per frame
	void Update () 
    {
        if(!m_started) return;
        transform.position = (Vector2)m_movementFraction + (Vector2)transform.position;
        if(m_endPoint.Equals(transform.position))
        {
            Destroy(this);
        }
	}
}
