using UnityEngine;
using System.Collections;
using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using System.Collections.Generic;

namespace Assets.scripts.InventoryScreen
{
    
    public class SystemSelectionBoxScript : SelectionBox<SubsystemTemplate>
    {
        private MarkerScript m_markedTexture;
        private static InventoryTextureHandler s_textureHandler;

        public static void Init(IEnumerable<SubsystemTemplate> systems, InventoryTextureHandler textureHandler)
        {
            Init(systems);
            s_textureHandler = textureHandler;
        }

        public override void Start()
        {
            base.Start();
            m_markedTexture = ((GameObject)Instantiate(Resources.Load("Marker"), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            m_markedTexture.Unmark();
        }

        protected override Rect ToRectangle(SubsystemTemplate item)
        {
            return new Rect(0, 0, 50, 50);
        }

        protected override GUIContent GetContent(SubsystemTemplate item)
        {
            if (item == null)
            {
                return new GUIContent(s_textureHandler.GetNullTexture(), "Empty");
            }
            return new GUIContent(s_textureHandler.GetSystemTexture(item), item.Name);
        }

        protected override void UpdateVisuals(SubsystemTemplate item)
        {
            if (item == null)
            {
                m_markedTexture.Unmark();
            }
            else
            {
                var renderer = m_markedTexture.gameObject.GetComponent<SpriteRenderer>();
                s_textureHandler.UpdateSystemMarkerTexture(item, renderer);
                m_markedTexture.Mark(transform.position);
                m_markedTexture.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
    }
    
}
