using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene
{
    public class LocationScript : MonoBehaviour
    {
        #region fields

        private bool m_seen;

        #endregion fields

        #region properties

        public EncounterTemplate Encounter { get; private set; }

        public IEnumerable<LocationScript> NextLocations { get; private set; }

        public IEnumerable<ChoiceTemplate> Choices { get; private set; }

        public bool DoneDisplayingContent { get; set; }

        #endregion properties

        #region public methods

        public static LocationScript CreateLocationScript(
            Vector2 coordinates,
            EncounterTemplate template,
            IEnumerable<LocationScript> nextLocations)
        {
            LocationScript newLocation = UnityHelper.Instantiate<LocationScript>(coordinates);
            newLocation.Init(template, nextLocations);
            return newLocation;
        }

        public void Display()
        { }

        public bool WasSeen()
        {
            return m_seen;
        }

        public void Seen()
        {
            m_seen = true;
        }

        #endregion public methods

        #region private methods

        private void Init(EncounterTemplate template, IEnumerable<LocationScript> nextLocations)
        {
            Encounter = template;
            NextLocations = nextLocations;

            if (Encounter.Choices == null)
            {
                return;
            }

            Choices = Encounter.Choices;
        }

        #endregion private methods
    }
}