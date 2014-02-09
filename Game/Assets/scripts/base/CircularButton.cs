using UnityEngine;
using System;

public class CircularButton : MarkerScript
{
    public Action Action { get; set; }
    public Action OnMouseOverProperty { get; set; }
    public Action OnMouseExitProperty { get; set; }

    void OnMouseOver()
    {
        OnMouseOverProperty();
    }

    void OnMouseExit()
    {
        OnMouseExitProperty();
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButton(0))
        {
            Action();
        }
    }
}

