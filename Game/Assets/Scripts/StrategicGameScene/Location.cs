#region

using System;
using System.Collections.Generic;

#endregion

namespace Assets.Scripts.StrategicGameScene
{
    public abstract class Location
    {
        public string Message { get; private set; }

        public IEnumerable<PlayerActionChoice> Choices { get; private set; }

        public IEnumerable<Location> NextLocations { get; private set; }

        public void Display()
        { }
    }

    public class PlayerActionChoice
    {
        public string Message { get; private set; }

        public Action Result { get; private set; }
    }
}