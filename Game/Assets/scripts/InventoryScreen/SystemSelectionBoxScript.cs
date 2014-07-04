using UnityEngine;
using System.Collections;
using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using System.Collections.Generic;

namespace Assets.scripts.InventoryScreen
{
    
    public class SystemSelectionBoxScript : SelectionBox<SubsystemTemplate>
    {

        private static InventoryTextureHandler s_textureHandler;

        public static void Init(IEnumerable<SubsystemTemplate> systems, InventoryTextureHandler textureHandler)
        {
            Init(systems);
            s_textureHandler = textureHandler;
        }

        protected override Rect ToRectangle(SubsystemTemplate item)
        {
            throw new System.NotImplementedException();
        }

        protected override GUIContent GetContent(SubsystemTemplate item)
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateVisuals(SubsystemTemplate item)
        {
            throw new System.NotImplementedException();
        }
    }
    
}
