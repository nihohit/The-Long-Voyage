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

        public override void Awake()
        {
            base.Awake();
            m_markedTexture = ((GameObject)Instantiate(Resources.Load("Marker"), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            m_markedTexture.Unmark();
        }

        protected override Texture2D GetTexture(SubsystemTemplate item)
        {
            if (item == null)
            {
                return s_textureHandler.GetNullTexture();
            }
            return s_textureHandler.GetSystemTexture(item);
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
