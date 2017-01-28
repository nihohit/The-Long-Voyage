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

		protected Image m_image;

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

		#region public methods

		public virtual void Awake()
		{
			m_image = transform.FindChild("Image").GetComponent<Image>();
			// gameObject.GetComponent<Button>().onClick.AddListener(this.ClickedOn);
		}

		#endregion public methods

		#region private methods

		protected virtual void UpdateVisuals(T item)
		{
			if (item == null)
			{
				if (m_image != null)
				{
					m_image.enabled = false;
				}
			}
			else
			{
				m_image.enabled = true;
				var texture = s_textureHandler.GetTexture(item);

				m_image.sprite = Sprite.Create(
					texture,
					m_image.sprite.rect,
					m_image.sprite.bounds.center);

				m_image.sprite.name = texture.name;
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
				if(value == base.SelectedItem || (base.SelectedItem != null && base.SelectedItem.Equals(value)))
				{
					return;
				}

				if (value != null)
				{
					s_selectableOptions.Remove(value);
				}

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
		public static void Init(List<T> items, ITextureHandler<T> textureHandler)
		{
			if (s_textureHandler != null)
			{
				return;
			}

			s_selectableOptions = items;
			s_textureHandler = textureHandler;
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
				if (Input.GetMouseButtonDown(0))
				{
					RemoveButtons();
				}
			}
		}

		public void ClickedOn()
		{
			Debug.Log(gameObject.name + " clicked on");
			RemoveButtons();
			CreateButtons();
			m_frameCounter = 5;
		}

		#endregion public methods

		#region private method

		private void CreateButtons()
		{
			var currentPosition = m_image.rectTransform.position;
			var currentWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentPosition.x, currentPosition.y, -Camera.main.transform.position.z));

			var bottomPartOfScreen = currentWorldPosition.y > Camera.main.transform.position.y;

			var choices = SelectedItem == null ?
				s_selectableOptions.Distinct() :
				s_selectableOptions.Union(new[] { SelectedItem }).Distinct();

			currentPosition = CreateButton(null, currentPosition, bottomPartOfScreen);

			// reverse the list if in the bottom part of the screen
			foreach (var item in bottomPartOfScreen ? choices.Reverse() : choices)
			{
				currentPosition = CreateButton(item, currentPosition, bottomPartOfScreen);
			}
		}

		// initializes a button, place it in its location, and return the updated LocationScript for the next button
		private Vector3 CreateButton(T item, Vector3 currentPosition, bool buttonsGoingDown)
		{
			var button = UnityHelper.Instantiate<Button>(currentPosition);
			button.onClick.AddListener(() => Debug.Log("button clicked"));
			button.transform.SetParent(gameObject.transform.parent);
			var image = button.transform.FindChild("Image").GetComponent<Image>();
			var scale = button.GetComponent<RectTransform>().localScale;
			button.GetComponent<RectTransform>().localScale = Vector3.one;

			var texture = GetTexture(item);
			button.onClick.AddListener(() => SelectedItem = item);
			button.name = texture.name + "Button";
			image.sprite = Sprite.Create(
					texture,
					image.sprite.rect,
					image.sprite.bounds.center);

			image.sprite.name = texture.name;

			return new Vector3(
				currentPosition.x,
				buttonsGoingDown ?
					currentPosition.y - (button.GetComponent<RectTransform>().rect.height / scale.y) :
					currentPosition.y + (button.GetComponent<RectTransform>().rect.height / scale.y),
				0);
		}

		private void RemoveButtons()
		{
			var buttons = transform.parent.GetComponentsInChildren<Button>().Where(button => button.name.Contains("Button"));

			foreach (var button in buttons)
			{
				button.DestroyGameObject(0.2f);
			}
		}

		#endregion private method

		#region abstract methods

		protected abstract Texture2D GetTexture(T item);

		#endregion abstract methods
	}

	#endregion DropDownSelectionBox
}