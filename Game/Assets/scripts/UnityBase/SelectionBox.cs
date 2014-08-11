using System.Collections.Generic;
using System.Linq;
using Assets.scripts.Base;
using UnityEngine;

namespace Assets.scripts.UnityBase
{
    #region SelectionBox

    /// <summary>
    /// A clickable box that offers a selection of possible items when clicked, and saves the chosen item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SelectionBox<T> : SimpleButton where T : class
    {
        #region fields

        // the selected item
        private T m_selectedItem;

        protected IUnityMarker m_markedTexture;

        protected static ITextureHandler<T> s_textureHandler;

        #endregion fields

        #region properties

        // when an item is selected, all displayed options must be removed and the box's visual need to be updated.
        public virtual T SelectedItem
        {
            get { return m_selectedItem; }
            set
            {
                m_selectedItem = value;
                UpdateVisuals(m_selectedItem);
            }
        }

        #endregion properties

        #region private methods

        protected virtual void UpdateVisuals(T item)
        {
            if (item == null)
            {
                m_markedTexture.Unmark();
            }
            else
            {
                var renderer = m_markedTexture.Renderer;
                s_textureHandler.UpdateMarkerTexture(item, renderer);
                m_markedTexture.Mark(transform.position);
                m_markedTexture.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }

        #endregion
    }

    #endregion

    #region DropDownSelectionBox

    public abstract class DropDownSelectionBox<T> : SelectionBox<T> where T : class
    {
        #region fields

        // a list of all available items
        protected static List<T> s_selectableOptions;

        // marks whether the mouse is still over the item.
        private bool m_mouseHover;

        // The displayed items
        private ButtonCluster m_buttons;

        // serves to prevent a click from registering twice
        private int m_frameCounter;

        #endregion fields

        #region properties

        // when an item is selected, all displayed options must be removed and the box's visual need to be updated.
        public override T SelectedItem
        {
            get { return base.SelectedItem; }
            set
            {
                s_selectableOptions.Remove(value);

                base.SelectedItem = value;
                RemoveButtons();
            }
        }

        #endregion properties

        #region public methods

        public virtual void Awake()
        {
            ClickableAction = ClickedOn;
            OnMouseExitAction = () => m_mouseHover = false;
            OnMouseOverAction = () => m_mouseHover = true;
        }

        // Update is called once per frame
        public virtual void Update()
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
                    RemoveButtons();
                }
            }
        }

        // sets all possible selection options
        public static void Init(List<T> items)
        {
            s_selectableOptions = items;
        }

        #endregion public methods

        #region private method

        private void ClickedOn()
        {
            RemoveButtons();
            m_buttons = new ButtonCluster(CreateButtons().Materialize());
            m_frameCounter = 5;
        }

        private IEnumerable<IUnityButton> CreateButtons()
        {
            var mousePosition = Input.mousePosition;
            var currentPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            var buttomPartOfScreen = currentPosition.y > Camera.main.transform.position.y;

            IUnityButton button;
            currentPosition = CreateButton(null, currentPosition, out button, buttomPartOfScreen);
            yield return button;

            var choices = s_selectableOptions.Select(ent => ent).Distinct();
            // reverse the list if in the bottom part of the screen
            foreach (var item in buttomPartOfScreen ? choices.Reverse() : choices)
            {
                currentPosition = CreateButton(item, currentPosition, out button, buttomPartOfScreen);
                yield return button;
            }
        }

        // initializes a button, place it in its location, and return the updated location for the next button
        private Vector3 CreateButton(T item, Vector3 currentPosition, out IUnityButton button, bool buttonsGoingDown)
        {
            var buttonObject = ((GameObject)Instantiate(Resources.Load("CircularButton"), currentPosition, Quaternion.identity));
            button = buttonObject.GetComponent<SimpleButton>();
            button.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            TextureHandler.ReplaceTexture(button.Renderer, GetTexture(item), "selection button");
            button.ClickableAction = () => SelectedItem = item;
            if (buttonsGoingDown)
            {
                return new Vector3(currentPosition.x, currentPosition.y - 0.2f * buttonObject.GetComponent<CircleCollider2D>().radius, 0);
            }
            return new Vector3(currentPosition.x, currentPosition.y + 0.2f * buttonObject.GetComponent<CircleCollider2D>().radius, 0);
        }

        private void RemoveButtons()
        {
            if (m_buttons != null)
            {
                m_buttons.DestroyCluster();
                m_buttons = null;
            }
        }

        #endregion private method

        #region abstract methods

        protected abstract Texture2D GetTexture(T item);

        #endregion
    }

    #endregion
}