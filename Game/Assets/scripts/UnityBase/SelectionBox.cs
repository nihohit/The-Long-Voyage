using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.scripts.Base;

namespace Assets.scripts.UnityBase
{
    public abstract class SelectionBox<T> : SimpleButton where T: class
    {
        #region fields

        private T m_selectedItem;
        protected static List<T> s_selectedOptions;
        private bool m_mouseHover;
        private ButtonCluster m_buttons;
        private int m_frameCounter;

        #endregion

        #region properties

        public T SelectedItem
        {
            get { return m_selectedItem; }
            set
            {
                if(value != null)
                {
                    Assert.AssertConditionMet(s_selectedOptions.Remove(value), "{0} was selected but isn't in {1}".FormatWith(value, s_selectedOptions));
                }
                if(m_selectedItem != null)
                {
                    s_selectedOptions.Add(m_selectedItem);
                }
                
                m_selectedItem = value;
                RemoveButtons();
                UpdateVisuals(m_selectedItem);
            }
        }

        #endregion

        #region public methods

        public virtual void Start()
        {
            ClickableAction = ClickedOn;
            OnMouseExitProperty = () => m_mouseHover = false;
            OnMouseOverProperty = () => m_mouseHover = true;
        }

        // Update is called once per frame
        public void Update()
        {
            if (m_frameCounter > 0)
            {
                m_frameCounter--;
            }
            else
            {
                //if the mouse is pressed and not on me, remove selection
                if (Input.GetMouseButtonDown(0) && !m_mouseHover)
                {
                    Debug.Log("removing buttons");
                    RemoveButtons();
                }
            }
        }

        public static void Init(IEnumerable<T> items)
        {
            s_selectedOptions = new List<T>(items);
        }

        #endregion

        private void ClickedOn()
        {
            RemoveButtons();
            m_buttons = new ButtonCluster(CreateButtons().Materialize());
            m_frameCounter = 30;
        }

        private IEnumerable<SimpleButton> CreateButtons()
        {
            var mousePosition = Input.mousePosition;
            var currentPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            SimpleButton button;
            currentPosition = CreateButton(null, currentPosition, out button);
            yield return button;
            foreach (var item in s_selectedOptions.Select(ent => ent).Distinct().Materialize())
            {
                currentPosition = CreateButton(item, currentPosition, out button);
                yield return button;
            }
        }

        private Vector3 CreateButton(T item, Vector3 currentPosition, out SimpleButton button)
        {
            var buttonObject = ((GameObject)Instantiate(Resources.Load("Button"), currentPosition, Quaternion.identity));
            button = buttonObject.GetComponent<SimpleButton>();
            buttonObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            TextureHandler.ReplaceTexture(buttonObject.GetComponent<SpriteRenderer>(), GetTexture(item), "selection button");
            button.ClickableAction = () =>  SelectedItem = item;
            return new Vector3(currentPosition.x, currentPosition.y - 0.2f * buttonObject.GetComponent<CircleCollider2D>().radius, 0);
        }

        private void RemoveButtons()
        {
            if(m_buttons != null)
            {
                m_buttons.DestroyCluster();
                m_buttons = null;
            }
        }

        protected abstract Texture2D GetTexture(T item);
        protected abstract void UpdateVisuals(T item);
    }
}
