using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.UnityBase;

namespace Assets.Scripts.StrategicGameScene {
  public class LocationScript : SimpleButton {
    #region fields

    private bool m_seen;

    #endregion fields

    #region properties

    public LocationInformation Information { get; set; }

    #endregion properties

    #region public methods

    public static LocationScript CreateLocationScript(
      LocationInformation information) {
      var newLocation = UnityHelper.Instantiate<LocationScript>(information.Coordinates);
      GlobalState.Instance.StrategicMap.StrategicMapTextureHandler.UpdateHexTexture(newLocation.Renderer, information.Biome);
      newLocation.name = "{0} {1}".FormatWith(information.Biome.ToString(), information.Coordinates);
      newLocation.Information = information;
      return newLocation;
    }

    public bool WasSeen() {
      return m_seen;
    }

    public void Seen() {
      m_seen = true;
    }

    #endregion public methods
  }
}