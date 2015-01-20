using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene
{
    public class LocationScript : MonoBehaviour
    {
        private bool m_seen;

        public LocationTemplate Template { get; private set; }

        public IEnumerable<LocationScript> NextLocations { get; private set; }

        public IEnumerable<PlayerActionChoice> Choices { get; private set; }

        public bool DoneDisplayingContent { get; set; }

        public void Display()
        { }

        public static LocationScript CreateLocationScript(
            Vector2 coordinates,
            LocationTemplate template,
            IEnumerable<LocationScript> nextLocations)
        {
            LocationScript newLocation = UnityHelper.Instantiate<LocationScript>(coordinates);
            newLocation.Init(template, nextLocations);
            return newLocation;
        }

        private void Init(LocationTemplate template, IEnumerable<LocationScript> nextLocations)
        {
            Template = template;
            NextLocations = nextLocations;

            if (Template.Choices == null)
            {
                return;
            }

            Choices = Template.Choices.Select(choice => new PlayerActionChoice(choice)).Materialize();
        }

        public bool WasSeen()
        {
            return m_seen;
        }

        public void Seen()
        {
            m_seen = true;
        }
    }
}