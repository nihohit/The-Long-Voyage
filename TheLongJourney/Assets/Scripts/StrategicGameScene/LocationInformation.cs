using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene {
  using Base;

  using UnityEngine;

  #region LocationInformation

  public class LocationInformation {
    #region properties

    public Vector2 Coordinates { get; private set; }

    public VignetteTemplate Vignette { get; private set; }

    public List<LocationInformation> ConnectedLocations { get; private set; }

    public bool WasVisited { get; set; }

    public bool WasSeen { get; set; }

    public Biome Biome { get; set; }

    #endregion properties

    #region constructors

    public LocationInformation(Vector2 coordinates, VignetteTemplate vignette, List<LocationInformation> connectedLocations) {
      Coordinates = coordinates;
      Vignette = vignette;
      ConnectedLocations = connectedLocations;
    }

    #endregion constructors

    public override string ToString() {
      return "{0} at {1}, {2} visited, {3}".FormatWith(
        Vignette,
        Coordinates,
        WasVisited ? "was" : "wasn't",
        WasSeen ? "Seen" : "unseen");
    }
  }

  #endregion LocationInformation
}