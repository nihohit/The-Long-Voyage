using System;
using UnityEngine;

public class CircularButton : MarkerScript
{
    #region properties

    public Action Action { get; set; }

    public Action OnMouseOverProperty { get; set; }

    public Action OnMouseExitProperty { get; set; }

    #endregion

    #region public methods

    public CircularButton()
    {
        //just in case those value aren't inserted afterwards
        OnMouseOverProperty = () => { };
        OnMouseExitProperty = () => { };
        Action = () => { };
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

    #endregion

    #region private and protected methods

    protected Action CheckIfClickIsOnUI(Action action)
    {
        return () =>
            {
                //TODO - check if the Unity bug which makes this necessary is fixed, and remove this.
                // Store the point where the user has clicked as a Vector3
                var mousePosition = Input.mousePosition;
                var clickPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
                // Retrieve all raycast hits from the click position and store them in an array called "hits"
                var rayHits = Physics2D.RaycastAll(clickPosition, new Vector2(0, 0));
                foreach (var rayHit in rayHits)
                {
                    var clickedComponent = rayHit.collider.gameObject;
                    if (clickedComponent.layer == LayerMask.NameToLayer("UI"))
                    {
                        var button = clickedComponent.GetComponent<CircularButton>();
                        button.Action();
                        return;
                    }
                }
                action();
            };
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

    private void OnMouseDown()
    {
        if (enabled)
        {
            Action();
        }
    }

    #endregion
}