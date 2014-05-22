using System;
using UnityEngine;

public class CircularButton : MarkerScript
{
    public Action Action { get; set; }

    public Action OnMouseOverProperty { get; set; }

    public Action OnMouseExitProperty { get; set; }

    public CircularButton()
    {
        //just in case those value aren't inserted afterwards
        OnMouseOverProperty = () => { };
        OnMouseExitProperty = () => { };
        Action = () => { };
    }

    private void OnMouseOver()
    {
        if (enabled)
        {
            OnMouseOverProperty();
        }
    }

    private void OnMouseExit()
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

    private void OnMouseDown()
    {
        if (enabled)
        {
            Action();
        }
    }
}