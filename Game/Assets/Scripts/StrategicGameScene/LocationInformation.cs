using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene
{
    using Assets.Scripts.Base;

    using UnityEngine;

    #region LocationInformation

    public class LocationInformation
    {
        #region properties

        public Vector2 Coordinates { get; private set; }

        public EncounterTemplate Encounter { get; private set; }

        public IEnumerable<LocationInformation> ConnectedLocations { get; private set; }

        public bool WasVisited { get; set; }

        public bool WasSeen { get; set; }

        #endregion properties

        #region constructors

        public LocationInformation(Vector2 coordinates, EncounterTemplate encounter, IEnumerable<LocationInformation> connectedLocations)
        {
            Coordinates = coordinates;
            Encounter = encounter;
            ConnectedLocations = connectedLocations;
        }

        #endregion constructors

        public override string ToString()
        {
            return "{0} at {1}, {2} visited, {3}".FormatWith(
                Encounter,
                Coordinates,
                WasVisited ? "was" : "wasn't",
                WasSeen ? "Seen" : "unseen");
        }
    }

    #endregion LocationInformation
}