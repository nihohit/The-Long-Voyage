using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.scripts.UnityBase;
using Assets.scripts.LogicBase;
using Assets.scripts.InterSceneCommunication;
using Assets.scripts.TacticalBattleScene;

namespace Assets.scripts.InventoryScreen
{
    public class EntitySelectionBoxScript : SelectionBox<SpecificEntity>
    {
        private static InventoryTextureHandler s_textureHandler;
        private MarkerScript m_markedTexture;

        public static void Init(IEnumerable<SpecificEntity> entities, InventoryTextureHandler textureHandler)
        {
            Init(entities);
            s_textureHandler = textureHandler;
        }

        public override void Start()
        {
            base.Start();
            m_markedTexture = ((GameObject)Instantiate(Resources.Load("Marker"), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            m_markedTexture.Unmark();
        }

        protected override Rect ToRectangle(SpecificEntity item)
        {
            return new Rect(0, 0, 40, 40);
        }

        protected override GUIContent GetContent(SpecificEntity item)
        {
            return new GUIContent(s_textureHandler.GetEntityTexture(item), item.Template.Name);
        }

        protected override void UpdateVisuals(SpecificEntity item)
        {
            if(item == null)
            {
                m_markedTexture.Unmark();
            }
            else
            {
                var renderer = m_markedTexture.gameObject.GetComponent<SpriteRenderer>();
                s_textureHandler.UpdateEntityMarkerTexture(item, renderer);
                m_markedTexture.Mark(transform.position);
            }
        }
    }
}
