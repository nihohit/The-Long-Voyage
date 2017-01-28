using System.Collections;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene.LoadupScreen
{
    using Assets.Scripts.Base;
    using Assets.Scripts.InterSceneCommunication;
    using Assets.Scripts.UnityBase;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine.UI;

    /*
     * inventory screen specifications -
     * empty boxes across the screen, with a scrollbar at the side which shows the available (unequipped) mechs and systems.
     * you can click on a box, and it will show a list of unequipped mechs to choose - clicking on one will place that mech in the box.
     * Alternatively, you can drag a mech from the sidebar into a box. Both systems work whether there's already a mech assigned to the box or not.
     * When a mech is assigned to a box, its portrait appears in the box, and smaller empty boxes appear, the amount of available subsystem space.
     * systems can be assigned the same way that mechs are - either by dragging them from the sidebar, or by clicking on a box.
     * When the screen is closed, the boxes are saved as EquippedEntity to the player's state.
     */

    public class LoadupScreenScript : MonoBehaviour
    {
        public Canvas MapScreen;

        private IEnumerable<EntitySelectionBoxScript> m_entitySelectionBoxes;

        public void SwitchToMapScreen()
        {
            gameObject.SetActive(false);
            MapScreen.gameObject.SetActive(true);
        }

        public void Awake()
        {
            m_entitySelectionBoxes = GetComponentsInChildren<EntitySelectionBoxScript>(true).ToList();
        }

        public void OnEnable()
        {
            InitializeSelectionBoxes();

            foreach (var ent in GlobalState.Instance.StrategicMap.State.EquippedEntities)
            {
                var firstEmptySelectionBox = m_entitySelectionBoxes.First(box => box.SelectedItem == null);
                firstEmptySelectionBox.SetEntity(ent);
            }

            GlobalState.Instance.StrategicMap.State.EquippedEntities.Clear();
        }

        public void OnDisable()
        {
            GlobalState.Instance.StrategicMap.State.EquippedEntities.AddRange(
                m_entitySelectionBoxes.Select(box => box.GetEquippedEntity()).ToList());
        }

        private void InitializeSelectionBoxes()
        {
            var textureHandler = GlobalState.Instance.StrategicMap.InventoryTextureHandler;
            EntitySelectionBoxScript.Init(GlobalState.Instance.StrategicMap.State.AvailableEntities, textureHandler);
            SystemSelectionBoxScript.Init(GlobalState.Instance.StrategicMap.State.AvailableSystems, textureHandler);
        }
    }
}