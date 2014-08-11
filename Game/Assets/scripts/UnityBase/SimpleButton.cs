using System;
using UnityEngine;

namespace Assets.scripts.UnityBase
{
    /// <summary>
    /// Button class that wraps button-relevant events with Action properties.
    /// Simplifies the creation of clickable objects in play.
    /// </summary>
    public class SimpleButton : MarkerScript, IUnityButton
    {
        #region properties

        public Action ClickableAction { get; set; }

        public virtual Action OnMouseOverAction { get; set; }

        public virtual Action OnMouseExitAction { get; set; }

        #endregion properties

        #region constructor

        public SimpleButton()
        {
            //just in case those value aren't inserted afterwards
            OnMouseOverAction = () => { };
            OnMouseExitAction = () => { };
            ClickableAction = () => { };
        }

        #endregion constructor

        #region overrides

        public override void Mark()
        {
            Mark(this.transform.position);
        }

        public override void Mark(Vector3 position)
        {
            base.Mark(position);
            this.GetComponent<Collider2D>().enabled = true;
        }

        public override void Unmark()
        {
            base.Unmark();
            this.GetComponent<Collider2D>().enabled = false;
        }

        #endregion overrides

        #region private and protected methods

        // This method is necessary due to a known Unity bug - sometimes a click is received
        // By a collider in a lower layer. This function sets a default check,
        // to see if there's a higher layer collider the click was aimed for.
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
                        var layerMask = LayerMask.NameToLayer("AddedUI");
                        if (clickedComponent.layer == layerMask)
                        {
                            var button = clickedComponent.GetComponent<SimpleButton>();
                            button.ClickableAction();
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
                OnMouseOverAction();
            }
        }

        private void OnMouseExit()
        {
            if (enabled)
                OnMouseExitAction();
        }

        private void OnMouseDown()
        {
            if (enabled)
            {
                ClickableAction();
            }
        }

        #endregion private and protected methods
    }
}