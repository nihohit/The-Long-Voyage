﻿using Assets.scripts.Base;
using Assets.scripts.InterSceneCommunication;
using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using UnityEngine;

namespace Assets.scripts.InventoryScreen
{
    /*
     * inventory screen specifications -
     * empty boxes across the screen, with a scrollbar at the side which shows the available (unequipped) mechs and systems.
     * you can click on a box, and it will show a list of unequipped mechs to choose - clicking on one will place that mech in the box.
     * Alternatively, you can drag a mech from the sidebar into a box. Both systems work whether there's already a mech assigned to the box or not.
     * When a mech is assigned to a box, its portrait appears in the box, and smaller empty boxes appear, the amount of available subsystem space.
     * systems can be assigned the same way that mechs are - either by dragging them from the sidebar, or by clicking on a box.
     * When the screen is closed, the boxes are saved as EquippedEntity to the player's state.
     */

    public class InventoryScreenScript : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
            if (GlobalState.StrategicMap == null)
            {
                SimpleConfigurationHandler.Init();
                EntityTemplate.Init();
                SubsystemTemplate.Init();
                GlobalState.StrategicMap = new StrategicMapInformation();
                GlobalState.StrategicMap.State = new PlayerState();
                var mechTemplate = EntityTemplate.GetTemplate(1);
                for (int i = 0; i < 4; i++)
                {
                    GlobalState.StrategicMap.State.AvailableEntities.Add(new SpecificEntity(mechTemplate));
                }
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        GlobalState.StrategicMap.State.AvailableSystems.Add(SubsystemTemplate.GetTemplate(i));
                    }
                }
                InventoryTextureHandler textureHandler = new InventoryTextureHandler();
                EntitySelectionBoxScript.Init(GlobalState.StrategicMap.State.AvailableEntities, textureHandler);
                SystemSelectionBoxScript.Init(GlobalState.StrategicMap.State.AvailableSystems, textureHandler);
            }
            // if we're after a battle, add the battle salvage to our eqiupment
            if (GlobalState.BattleSummary != null)
            {
                var battleResult = GlobalState.BattleSummary;
                GlobalState.StrategicMap.State.AvailableEntities.AddRange(battleResult.SalvagedEntities);
                GlobalState.StrategicMap.State.AvailableSystems.AddRange(battleResult.SalvagedSystems);
                GlobalState.StrategicMap.State.EquippedEntities.Clear();
                GlobalState.StrategicMap.State.EquippedEntities.AddRange(battleResult.SurvivingEntities);
                EntitySelectionBoxScript.TryAcquireEntities();
                EntitySelectionBoxScript.Init(GlobalState.StrategicMap.State.AvailableEntities);
                SystemSelectionBoxScript.Init(GlobalState.StrategicMap.State.AvailableSystems);
            }
            var buttonObject = ((GameObject)Instantiate(Resources.Load("CircularButton"), Vector3.zero, Quaternion.identity));
            buttonObject.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            var button = buttonObject.GetComponent<SimpleButton>();
            button.ClickableAction = () => Application.LoadLevel("TacticalBattleScene");
        }
    }
}