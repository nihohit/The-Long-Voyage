using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.scripts.Base;

namespace Assets.scripts.UnityBase
{
    public abstract class SelectionBox<T> : SimpleButton where T: class
    {
        private T m_selectedItem;
        protected static List<T> s_selectedOptions;
        private bool m_clickedOn;
        private bool m_mouseHover;
        private Vector3 m_mouseClickPoint;

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
                m_clickedOn = false;
                UpdateVisuals(m_selectedItem);
            }
        }

        public virtual void Start()
        {
            m_clickedOn = false;
            ClickableAction = ClickedOn;
            OnMouseExitProperty = () => m_mouseHover = false;
            OnMouseOverProperty = () => m_mouseHover = true;
        }

        // Update is called once per frame
        public void Update()
        {
            //if the mouse is pressed and not on me, remove selection
            if(Input.GetMouseButtonDown(0) && !m_mouseHover)
            {
                m_clickedOn = false;
            }
        }

        void OnGUI()
        {
            if (m_clickedOn)
            {
                var cameraTop = Camera.main.transform.position.y + Camera.main.pixelHeight;
                var currentPosition = new Vector3(m_mouseClickPoint.x, cameraTop - m_mouseClickPoint.y, m_mouseClickPoint.z);
                currentPosition = CreateGuiButton(null, currentPosition);
                foreach(var item in s_selectedOptions.Select(ent => ent).Distinct().Materialize())
                {
                    currentPosition = CreateGuiButton(item, currentPosition);
                }
                GUI.Label(new Rect(Input.mousePosition.x + 20, cameraTop - Input.mousePosition.y, 100, 40), GUI.tooltip);
            }
        }

        public static void Init(IEnumerable<T> items)
        {
            s_selectedOptions = new List<T>(items);
        }

        private Vector3 CreateGuiButton(T item, Vector3 currentPosition)
        {
            var rect = ToRectangle(item);
            if (GUI.Button(new Rect(currentPosition.x, currentPosition.y, rect.height, rect.width), GetContent(item)))
            {
                SelectedItem = item;
            }
            return new Vector3(currentPosition.x, currentPosition.y + rect.height, 0);
        }

        private void ClickedOn()
        {
            if (!m_clickedOn)
            {
                m_clickedOn = true;
                m_mouseClickPoint = Input.mousePosition;
            }
        }

        protected abstract Rect ToRectangle(T item);
        protected abstract GUIContent GetContent(T item);
        protected abstract void UpdateVisuals(T item);
    }
}
