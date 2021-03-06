using System.Collections.Generic;
using Assets.Scripts.StrategicGameScene;
using Assets.Scripts.StrategicGameScene.LoadupScreen;

namespace Assets.Scripts.InterSceneCommunication {
  /// <summary>
  /// Information relevant to the strategic gameplay, to be passed from one vignette to the next.
  /// </summary>
  public class StrategicMapInformation {
    public PlayerState State { get; private set; }

    public LocationInformation CurrentLocation { get; set; }

    public InventoryTextureHandler InventoryTextureHandler { get; private set; }

    public StrategicMapTextureHandler StrategicMapTextureHandler { get; private set; }

    public List<LocationInformation> Map { get; private set; }

    public StrategicMapInformation(string playerName) {
      State = new PlayerState(playerName);
      InventoryTextureHandler = new InventoryTextureHandler();
      StrategicMapTextureHandler = new StrategicMapTextureHandler();
      Map = new List<LocationInformation>();
    }
  }
}