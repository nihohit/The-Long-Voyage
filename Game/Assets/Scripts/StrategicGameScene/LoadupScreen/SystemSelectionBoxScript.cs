using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene.LoadupScreen
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

        protected override Texture2D GetTexture(SubsystemTemplate item)
        {
            if (item == null)
            {
                return s_textureHandler.GetNullTexture();
            }

            return s_textureHandler.GetTexture(item);
        }
    }
}