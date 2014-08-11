using System.Collections.Generic;
using System.Linq;
using Assets.scripts.Base;
using Assets.scripts.InterSceneCommunication;
using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using UnityEngine;

namespace Assets.scripts.InventoryScreen
{
    /// <summary>
    /// A selection box for entities, that creates selection boxes for systems when an entity is created.
    /// Also is automatically populated if there are chosen equipped entities,
    /// and populates the equipped entities list when leaving the inventory scene.
    /// </summary>
    public class EntitySelectionBoxScript : DropDownSelectionBox<SpecificEntity>
    {
        #region fields

        private IEnumerable<SystemSelectionBoxScript> m_systems;
        private static bool s_equippedEntitiesWaiting = false;

        #endregion 
        
        #region public methods

        public static void Init(List<SpecificEntity> entities, InventoryTextureHandler textureHandler)
        {
            Init(entities);
            s_textureHandler = textureHandler;
        }

        public static void TryAcquireEntities()
        {
            s_equippedEntitiesWaiting = true;
        }

        #endregion

        #region private methods

        public override void Awake()
        {
            base.Awake();
            ClickableAction = CheckIfClickIsOnUI(ClickableAction);
            m_markedTexture = ((GameObject)Instantiate(Resources.Load("Marker"), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            m_markedTexture.Unmark();
        }

        public override void Update()
        {
            if (s_equippedEntitiesWaiting)
            {
                TryGetEntity();
            }
            base.Update();
        }

        private void TryGetEntity()
        {
            //locking a shared object
            lock (s_textureHandler)
            {
                var firstEntity = GlobalState.StrategicMap.State.EquippedEntities.FirstOrDefault();
                if (firstEntity == null)
                {
                    s_equippedEntitiesWaiting = false;
                    return;
                }

                GlobalState.StrategicMap.State.EquippedEntities.Remove(firstEntity);
                SelectedItem = firstEntity.Entity;
                var systemsArray = firstEntity.Subsystems.ToArray();
                var selectionBoxesArray = m_systems.ToArray();
                for (int i = 0; i < systemsArray.Length; i++)
                {
                    selectionBoxesArray[i].SelectedItem = systemsArray[i];
                }
            }
        }

        private void OnDisable()
        {
            if (SelectedItem != null)
            {
                lock (GlobalState.StrategicMap)
                {
                    GlobalState.StrategicMap.State.EquippedEntities.Add(
                        new EquippedEntity(SelectedItem, m_systems.Select(systemBox => systemBox.SelectedItem)));
                }
            }
        }

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
            if (m_systems != null)
            {
                m_systems.ForEach(system => system.SelectedItem = null);
                m_systems.ForEach(system => system.DestroyGameObject());
                m_systems = null;
            }
            if (item == null)
            {
                m_markedTexture.Unmark();
            }
            else
            {
                var renderer = m_markedTexture.Renderer;
                s_textureHandler.UpdateMarkerTexture(item, renderer);
                m_markedTexture.Mark(UpperLeftCornerLocation());
                m_markedTexture.Scale = new Vector3(0.2f, 0.2f, 0.2f);
                m_systems = CreateSystemSelectionBoxes(item.Template.SystemSlots).Materialize();
            }
        }

        private IEnumerable<SystemSelectionBoxScript> CreateSystemSelectionBoxes(int systemSlotsAmount)
        {
            var center = transform.position;
            var size = gameObject.GetComponent<BoxCollider2D>().size;
            var scale = transform.localScale;
            var scaledSize = new Vector2(size.x * scale.x, size.y * scale.y);
            for (int i = 0; i < systemSlotsAmount; i++)
            {
                Vector3 position = default(Vector3);
                switch (i)
                {
                    case (0):
                        position = new Vector3(center.x + scaledSize.x / 3.5f, center.y + scaledSize.y / 4, 0);
                        break;

                    case (1):
                        position = new Vector3(center.x + scaledSize.x / 3.5f, center.y - scaledSize.y / 4, 0);
                        break;

                    case (2):
                        position = new Vector3(center.x, center.y - scaledSize.y / 4, 0);
                        break;

                    case (3):
                        position = new Vector3(center.x - scaledSize.x / 3.5f, center.y - scaledSize.y / 4, 0);
                        break;
                }
                var systemBox = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("SystemSelectionBox"), position, Quaternion.identity));
                yield return systemBox.GetComponent<SystemSelectionBoxScript>();
            }
        }

        private Vector3 UpperLeftCornerLocation()
        {
            var center = transform.position;
            var size = gameObject.GetComponent<BoxCollider2D>().size;
            var scale = transform.localScale;
            var scaledSize = new Vector2(size.x * scale.x, size.y * scale.y);
            var leftMostEdge = center.x - scaledSize.x / 4;
            var upperMostEdge = center.y + scaledSize.y / 4;
            return new Vector3(leftMostEdge, upperMostEdge, 0);
        }

        #endregion
    }
}