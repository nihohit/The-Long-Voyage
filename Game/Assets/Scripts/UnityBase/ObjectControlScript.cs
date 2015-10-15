using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Assets.Scripts.Base;

namespace Assets.Scripts.UnityBase
{
	public class ObjectControlScript : MonoBehaviour
	{
		private Dictionary<string, Sprite> m_spriteDict;
		private Vector3 m_objectPosition;
		public float Radius;
		public float Size;

		public bool IsShowing { get; set; }

		public void Start()
		{
			var sprites = Resources.LoadAll<Sprite>("Buttons");
			m_spriteDict = sprites.ToDictionary(
				sprite => sprite.name,
				sprite => sprite);
		}

		public void Reset()
		{
			foreach (Transform gameObject in this.transform)
			{
				Destroy(gameObject.gameObject);
			}

			IsShowing = false;
		}

		public void SetLocation()
		{
			SetLocation(m_objectPosition);
		}

		public void SetLocation(Vector3 position)
		{
			m_objectPosition = position;
			IsShowing = true;
			var cameraPosition = Camera.main.transform.position;
			var vector = position - cameraPosition;

			var yDifference = (position.y - this.transform.position.y) / (position.y - cameraPosition.y);

			this.transform.position = position - (vector * yDifference);
		}

		public void CreateButtons(IEnumerable<ButtonDescription> buttons)
		{
			var buttonsIndentationAngle = 360f / buttons.Count();
			var buttonIndentationVector = Vector3.right * Radius;

			foreach (var button in buttons)
			{
				CreateButton(buttonIndentationVector, button);
				buttonIndentationVector = Quaternion.AngleAxis(buttonsIndentationAngle, transform.up) * buttonIndentationVector;
			}
		}

		private void CreateButton(Vector3 position, ButtonDescription description)
		{
			GameObject buttonObject = new GameObject();
			var rect = buttonObject.AddComponent<RectTransform>();
			var button = buttonObject.AddComponent<Button>();
			var image = buttonObject.AddComponent<Image>();

			buttonObject.transform.position = position;
			buttonObject.transform.SetParent(this.transform, false);
			buttonObject.name = description.ImageName + "Button";

			rect.SetSize(new Vector2(Size, Size));
			if (description.IsAvailable)
			{
				button.onClick.AddListener(new UnityAction
					(() =>
					{
						description.Action();
						Reset();
					}));
			}
			else
			{
				button.targetGraphic = image;
				button.interactable = false;
			}

			image.sprite = m_spriteDict.Get(description.ImageName);
		}
	}

	public class ButtonDescription
	{
		public string ImageName { get; set; }
		public Action Action { get; set; }
		public bool IsAvailable { get; set; }
	}
}