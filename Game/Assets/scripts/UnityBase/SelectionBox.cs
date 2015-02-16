using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnityBase
{
    #region SelectionBox

    /// <summary>
    /// A clickable box that offers a selection of possible items when clicked, and saves the chosen item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SelectionBox<T> : MonoBehaviour where T : class
    {
        #region fields

        protected static ITextureHandler<T> s_textureHandler;

        protected static object s_sharedLock = new object();

        // the selected item
        private T m_selectedItem;

        #endregion fields

        #region properties

        // when an item is selected, all displayed options must be removed and the box's visual need to be updated.
        public virtual T SelectedItem
        {
            get
            {
                return m_selectedItem;
            }
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
            var image = transform.FindChild("Image").GetComponent<Image>();

            if (item == null)
            {
                image.enabled = false;
            }
            else
            {
                image.enabled = true;
                var texture = s_textureHandler.GetTexture(item);
                var size = Convert.ToInt32(image.sprite.bounds.size.x);
                texture.wrapMode = TextureWrapMode.Clamp;

                image.sprite = Sprite.Create(
                    texture,
                    image.sprite.rect,
                    image.sprite.bounds.center);

                image.sprite.name = texture.name;
            }
        }

        #endregion private methods
    }

    #endregion SelectionBox

    #region DropDownSelectionBox

    public abstract class DropDownSelectionBox<T> : SelectionBox<T> where T : class
    {
        #region fields

        // a list of all available items
        protected static List<T> s_selectableOptions;

        // marks whether the last mouse click registered on the button
        private bool m_clickedOn;

        // The displayed items
        private ButtonCluster m_buttons;

        // serves to prevent a click from registering twice
        private int m_frameCounter;

        #endregion fields

        #region properties

        // when an item is selected, all displayed options must be removed and the box's visual need to be updated.
        public override T SelectedItem
        {
            get
            {
                return base.SelectedItem;
            }

            set
            {
                s_selectableOptions.Remove(value);
                if (base.SelectedItem != null)
                {
                    s_selectableOptions.Add(base.SelectedItem);
                }

                base.SelectedItem = value;
                RemoveButtons();
            }
        }

        #endregion properties

        #region public methods

        // sets all possible selection options
        public static void Init(List<T> items)
        {
            s_selectableOptions = items;
        }

        public virtual void Awake()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(this.ClickedOn);
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
                // if the mouse is pressed and not on me, remove selection
                if (Input.GetMouseButtonDown(0) && !m_clickedOn)
                {
                    RemoveButtons();
                }
            }

            m_clickedOn = false;
        }

        public void ClickedOn()
        {
            m_clickedOn = true;
            RemoveButtons();
            m_buttons = new ButtonCluster(CreateButtons().Materialize());
            m_frameCounter = 5;
        }

        #endregion public methods

        #region private method

        private IEnumerable<IUnityButton> CreateButtons()
        {
            var mousePosition = Input.mousePosition;
            var currentPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            var buttomPartOfScreen = currentPosition.y > Camera.main.transform.position.y;

            IUnityButton button;
            currentPosition = CreateButton(null, currentPosition, out button, buttomPartOfScreen);
            yield return button;

            var choices = s_selectableOptions.Distinct();

            // reverse the list if in the bottom part of the screen
            foreach (var item in buttomPartOfScreen ? choices.Reverse() : choices)
            {
                currentPosition = CreateButton(item, currentPosition, out button, buttomPartOfScreen);
                yield return button;
            }
        }

        // initializes a button, place it in its location, and return the updated LocationScript for the next button
        private Vector3 CreateButton(T item, Vector3 currentPosition, out IUnityButton button, bool buttonsGoingDown)
        {
            var buttonObject = UnityHelper.Instantiate<SimpleButton>(currentPosition, "CircularButton");
            buttonObject.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            button = buttonObject;
            var texture = GetTexture(item);
            TextureHandler.ReplaceTexture(button.Renderer, texture);
            button.ClickableAction = () => SelectedItem = item;
            buttonObject.name = texture.name + "Button";
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

        #endregion abstract methods
    }

    #endregion DropDownSelectionBox
}