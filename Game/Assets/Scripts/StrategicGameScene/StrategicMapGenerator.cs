using Assets.Scripts.Base;
using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene
{
    using UnityEngine;

    #region StrategicMapGenerator

    public class StrategicMapGenerator
    {
        #region public methods

        public LocationInformation GenerateStrategicMap()
        {
            Vector2 currentLocation = new Vector2(377, 370);
            LocationInformation first = new LocationInformation(
                currentLocation,
                LocationTemplateConfigurationStorage.Instance.GetAllConfigurations().ChooseRandomValue(),
                new List<LocationInformation>());

            var dict = new Dictionary<Vector2, LocationInformation>();
            dict.Add(currentLocation, first);

            GeneratePoints(currentLocation, dict, 0);

            return first;
        }

        private void GeneratePoints(Vector2 location, Dictionary<Vector2, LocationInformation> otherLocations, int depth)
        {
            if (depth == 4)
            {
                return;
            }

            LocationInformation current = otherLocations.TryGetOrAdd(
                location,
                () => new LocationInformation(
                        location,
                        LocationTemplateConfigurationStorage.Instance.GetAllConfigurations().ChooseRandomValue(),
                        new List<LocationInformation>()));

            var newVector = new Vector2(location.x + 3, location.y - 1);
            this.AddToConnectedLocations(current, newVector, otherLocations);
            GeneratePoints(newVector, otherLocations, depth + 1);

            newVector = new Vector2(location.x + 3, location.y + 1);
            this.AddToConnectedLocations(current, newVector, otherLocations);
            GeneratePoints(newVector, otherLocations, depth + 1);
        }

        private void AddToConnectedLocations(LocationInformation first, Vector2 location, Dictionary<Vector2, LocationInformation> otherLocations)
        {
            var second = otherLocations.TryGetOrAdd(
                location,
                () => new LocationInformation(
                        location,
                        LocationTemplateConfigurationStorage.Instance.GetAllConfigurations().ChooseRandomValue(),
                        new List<LocationInformation>()));

            ((List<LocationInformation>)first.ConnectedLocations).Add(second);
            ((List<LocationInformation>)second.ConnectedLocations).Add(first);
        }

        #endregion public methods
    }

    #endregion StrategicMapGenerator
}