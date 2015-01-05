using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.InventoryScreen
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
            InitializeSelectionBoxes();

            var button = UnityHelper.Instantiate<SimpleButton>("CircularButton");
            button.gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            button.ClickableAction = () => Application.LoadLevel("StrategicMapScene");
        }

        private void InitializeSelectionBoxes()
        {
            var textureHandler = GlobalState.Instance.StrategicMap.InventoryTextureHandler;
            EntitySelectionBoxScript.TryAcquireEntities();
            EntitySelectionBoxScript.Init(GlobalState.Instance.StrategicMap.State.AvailableEntities, textureHandler);
            SystemSelectionBoxScript.Init(GlobalState.Instance.StrategicMap.State.AvailableSystems, textureHandler);
        }
    }
}