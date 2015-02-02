﻿using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.InventoryScreen
{
    /// <summary>
    /// A selection box for entities' subsystems
    /// </summary>
    public class SystemSelectionBoxScript : DropDownSelectionBox<SubsystemTemplate>
    {
        public static void Init(List<SubsystemTemplate> systems, InventoryTextureHandler textureHandler)
        {
            Init(systems);
            s_textureHandler = textureHandler;
        }

        // this is called when the game object is created
        public override void Awake()
        {
            base.Awake();
            m_markedTexture = UnityHelper.Instantiate<MarkerScript>();
            m_markedTexture.Unmark();
        }

        protected override Texture2D GetTexture(SubsystemTemplate item)
        {
            if (item == null)
            {
                return s_textureHandler.GetNullTexture();
            }

            return s_textureHandler.GetTexture(item);
        }

        protected override void UpdateVisuals(SubsystemTemplate item)
        {
            if (item == null)
            {
                m_markedTexture.Unmark();
            }
            else
            {
                var textureRenderer = m_markedTexture.Renderer;
                s_textureHandler.UpdateMarkerTexture(item, textureRenderer);
                m_markedTexture.Mark(transform.position);
                m_markedTexture.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
    }
}