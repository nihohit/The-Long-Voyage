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
                m_systems.Select(systemBox => systemBox.SelectedItem).Where(system => system != null).ToList());

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

                int i = 0;

                foreach (var system in m_systems)
                {
                    if (item != null && i < item.Template.SystemSlots)
                    {
                        i++;
                        system.gameObject.SetActive(true);
                    }
                    else
                    {
                        system.SelectedItem = null;
                        system.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void FindSystems()
        {
            if (m_systems == null)
            {
                m_systems = gameObject.GetComponentsInChildren<SystemSelectionBoxScript>(true).ToList();
                m_systems.ForEach(system => system.gameObject.SetActive(false));
            }
        }

        #endregion private methods
    }
}