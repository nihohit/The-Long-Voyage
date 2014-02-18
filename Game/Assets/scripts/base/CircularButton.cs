using UnityEngine;
using System;

public class CircularButton : MarkerScript
{
    public Action Action { get; set; }
    public Action OnMouseOverProperty { get; set; }
    public Action OnMouseExitProperty { get; set; }

    void OnMouseOver()
    {
        if (enabled)
        {
            OnMouseOverProperty();
            if (Input.GetMouseButton(1))
            {
                TacticalState.SelectedHex = null;
            }
        }
    }

    void OnMouseExit()
    {
        if (enabled)
            OnMouseExitProperty();
    }

    public override void Mark()
    {
        base.Mark();
        this.GetComponent<Collider2D>().enabled = true;
    }

    public override void Unmark()
    {
        base.Unmark();
        this.GetComponent<Collider2D>().enabled = false;
    }

    void OnMouseDown()
    {
        if (enabled)
        {
            Action();
        }
    }
}

