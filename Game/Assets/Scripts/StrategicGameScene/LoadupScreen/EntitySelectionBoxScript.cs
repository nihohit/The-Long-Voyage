using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene.LoadupScreen
{
    using UnityEngine.UI;

    /// <summary>
    /// A selection box for entities, that creates selection boxes for systems when an entity is created.
    /// Also is automatically populated if there are chosen equipped entities,
    /// and populates the equipped entities list when leaving the inventory scene.
    /// </summary>
    public class EntitySelectionBoxScript : DropDownSelectionBox<SpecificEntity>
    {
        #region fields

        private IEnumerable<SystemSelectionBoxScript> m_systems;

        #endregion fields

        #region public methods

        public static void Init(List<SpecificEntity> entities, InventoryTextureHandler textureHandler)
        {
            Init(entities);
            s_textureHandler = textureHandler;
        }

        public override void Awake()
        {
            base.Awake();
            lock (this)
            {
                FindSystems();
            }
        }

        public EquippedEntity GetEquippedEntity()
        {
            var ent = new EquippedEntity(
                SelectedItem,
                m_systems.Select(systemBox => systemBox.SelectedItem).Where(system => system != null));

            SelectedItem = null;
            foreach (var box in m_systems)
            {
                box.SelectedItem = null;
            }

            return ent;
        }

        public void SetEntity(EquippedEntity ent)
        {
            SelectedItem = ent.InternalEntity;

            foreach (var system in ent.Subsystems)
            {
                m_systems.First(box => box.SelectedItem == null).SelectedItem = system;
            }
        }

        #endregion public methods

        #region private methods

        protected override Texture2D GetTexture(SpecificEntity item)
        {
            if (item == null)
            {
                return s_textureHandler.GetNullTexture();
            }

            return s_textureHandler.GetTexture(item);
        }

        protected override void UpdateVisuals(SpecificEntity item)
        {
            base.UpdateVisuals(item);

            lock (this)
            {
                this.FindSystems();

                if (item == null)
                {
                    m_systems.ForEach(system => system.gameObject.SetActive(false));
                }
                else
                {
                    m_systems.Take(item.Template.SystemSlots).ForEach(system => system.gameObject.SetActive(true));
                }
            }
        }

        private void FindSystems()
        {
            if (m_systems == null)
            {
                m_systems = gameObject.GetComponentsInChildren<SystemSelectionBoxScript>(true).Materialize();
                m_systems.ForEach(system => system.gameObject.SetActive(false));
            }
        }

        #endregion private methods
    }
}