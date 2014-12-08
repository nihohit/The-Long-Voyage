#region

using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;

#endregion

namespace Assets.Scripts.StrategicGameScene
{
    public class Location : SimpleButton
    {
        public string Message { get; private set; }

        public IEnumerable<PlayerActionChoice> Choices { get; private set; }

        public IEnumerable<Location> NextLocations { get; private set; }

        public void Display()
        { }

        public void Init()
        {}
    }

    public class PlayerActionChoice
    {
        #region fields

        private readonly float m_value;

        private readonly ChoiceResults m_results;

        private readonly string m_resultsDescription;

        #endregion

        #region properties

        public string Description { get; private set; }

        #endregion

        public PlayerActionChoice(string description, float value, ChoiceResults results, string resultDescription)
        {
            m_value = value;
            m_results = results;
            m_resultsDescription = resultDescription;
            Description = description;
        }
    }
}