using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.UnityBase
{
    public abstract class SelectionBox<T> : SimpleButton
    {
        protected static List<T> s_selectedOptions;
        protected T SelectedItem { get; set; }
        private bool m_selected;
        private bool m_mouseHover;

        void Start()
        {
            m_selected = false;
            ClickableAction = () => m_selected = true;
            OnMouseExitProperty = () => m_mouseHover = false;
            OnMouseOverProperty = () => m_mouseHover = true;
        }

        // Update is called once per frame
        void Update()
        {
            //if the mouse is pressed and not on me, remove selection
            if(Input.GetMouseButtonDown(0) && !m_mouseHover)
            {
                m_selected = false;
            }
        }

        void OnGUI()
        {
            if(m_selected)
            {

            }
        }

        public static void Init(IEnumerable<T> items)
        {
            s_selectedOptions = new List<T>(items);
        }
    }
}
