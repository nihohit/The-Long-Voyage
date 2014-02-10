using UnityEngine;
using System;

public class CircularButton : MarkerScript
{
    public Action Action { get; set; }
    public Action OnMouseOverProperty { get; set; }
    public Action OnMouseExitProperty { get; set; }

    void OnMouseOver()
    {
        if(enabled)
            OnMouseOverProperty();
    }

    void OnMouseExit()
    {
        if (enabled)
            OnMouseExitProperty();
    }

    void OnMouseDown()
    {
        if (enabled)
        {
            if (Input.GetMouseButton(0))
            {
                Action();
            }
        }
    }
}

