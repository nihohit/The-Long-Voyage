using Assets.Scripts.StrategicGameScene;
using Assets.Scripts.StrategicGameScene.LoadupScreen;

namespace Assets.Scripts.InterSceneCommunication
{
    /// <summary>
    /// Information relevant to the strategic gameplay, to be passed from one encounter to the next.
    /// </summary>
    public class StrategicMapInformation
    {
        public PlayerState State { get; private set; }

        public LocationInformation CurrentLocation { get; set; }

        public InventoryTextureHandler InventoryTextureHandler { get; private set; }

        public StrategicMapInformation(string playerName)
        {
            State = new PlayerState(playerName);
            InventoryTextureHandler = new InventoryTextureHandler();
        }
    }
}