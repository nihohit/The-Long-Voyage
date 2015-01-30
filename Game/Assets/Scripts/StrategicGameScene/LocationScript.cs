using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene
{
    public class LocationScript : SimpleButton
    {
        #region fields

        private bool m_seen;

        #endregion fields

        #region properties

        public LocationInformation Information { get; set; }

        #endregion properties

        #region public methods

        public static LocationScript CreateLocationScript(
            LocationInformation information)
        {
            LocationScript newLocation = UnityHelper.Instantiate<LocationScript>(information.Coordinates);
            newLocation.Information = information;
            return newLocation;
        }

        public bool WasSeen()
        {
            return m_seen;
        }

        public void Seen()
        {
            m_seen = true;
        }

        #endregion public methods
    }
}