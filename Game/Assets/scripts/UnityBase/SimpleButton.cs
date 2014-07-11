using System;
using UnityEngine;
using Assets.scripts.Base;

namespace Assets.scripts.UnityBase
{
    public class SimpleButton : MarkerScript
    {
        #region properties

        public Action ClickableAction { get; set; }

        public virtual Action OnMouseOverProperty { get; set; }

        public virtual Action OnMouseExitProperty { get; set; }

        #endregion properties

        #region public methods

        public SimpleButton()
        {
            //just in case those value aren't inserted afterwards
            OnMouseOverProperty = () => { };
            OnMouseExitProperty = () => { };
            ClickableAction = () => { };
        }

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

        #endregion public methods

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
                ClickableAction();
            }
        }

        #endregion private and protected methods
    }
}