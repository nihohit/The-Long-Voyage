using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UnityBase
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
            Mark(transform.position);
        }

        public override void Mark(Vector3 position)
        {
            base.Mark(position);
            GetComponent<Collider2D>().enabled = true;
        }

        public override void Unmark()
        {
            base.Unmark();
            GetComponent<Collider2D>().enabled = false;
        }

        #endregion overrides

        #region private and protected methods

        // This method is necessary due to a known Unity bug - sometimes a click is received
        // By a collider in a lower layer. This function sets a default check,
        // to see if there's a higher layer collider the click was aimed for.
        protected Action CheckIfClickIsOnUI(Action action)
        {
            return CheckIfClickIsOnLayer(action, "AddedUI");
        }

        // This method is necessary due to a known Unity bug - sometimes a click is received
        // By a collider in a lower layer. This function sets a default check,
        // to see if there's a higher layer collider the click was aimed for.
        protected Action CheckIfClickIsOnLayer(Action action, string layer)
        {
            return () =>
                {
                    //TODO - check if the Unity bug which makes this necessary is fixed, and remove this.
                    // Store the point where the user has clicked as a Vector3
                    var mousePosition = Input.mousePosition;
                    var clickPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

                    var layerMask = LayerMask.NameToLayer(layer);

                    var UIOnPoint = ObjectOnPoint(clickPosition, layerMask);

                    if (UIOnPoint != null)
                    {
                        var button = UIOnPoint.GetComponent<SimpleButton>();
                        button.ClickableAction();
                        return;
                    }
                    action();
                };
        }

        //returns an object on a layer, in a certain point
        protected GameObject ObjectOnPoint(Vector3 point, Int32 layerMask)
        {
            var rayHits = Physics2D.RaycastAll(point, new Vector2(0, 0));
            return rayHits.Select(rayHit => rayHit.collider.gameObject).
                FirstOrDefault(gameObject => gameObject.layer == layerMask);
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