using System.Linq;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using Assets.Scripts.Base;

namespace Assets.Scripts.StrategicGameScene
{
    public class LocationTemplate
    {
        public string Message { get; private set; }

        public IEnumerable<PlayerActionChoiceTemplate> Choices { get; private set; }

        public LocationTemplate(string message, IEnumerable<PlayerActionChoiceTemplate> choices)
        {
            Message = message;
            const int maxChoices = 4;
            Assert.EqualOrLesser(choices.Count(), maxChoices,
                "Too many choices in location {0}. Maximum allowed is {1}".FormatWith(Message, maxChoices));
            Choices = choices;
        }
    }

    public class Location
    {
        public LocationTemplate Template { get; private set; }

        public IEnumerable<Location> NextLocations { get; private set; }

        public IEnumerable<PlayerActionChoice> Choices { get; private set; }

        public bool DoneChoosing { get { return Choices.Any(choice => choice.Chosen); } }

        public void Display()
        { }

        public Location(LocationTemplate template, IEnumerable<Location> nextLocations)
        {
            Template = template;
            NextLocations = nextLocations;
            Choices = Template.Choices.Select(choice => new PlayerActionChoice(choice)).Materialize();
        }
    }

    public class PlayerActionChoice
    {
        public PlayerActionChoiceTemplate Template { get; private set; }

        public bool Chosen { get; private set; }

        public PlayerActionChoice(PlayerActionChoiceTemplate template)
        {
            Template = template;
            Chosen = false;
        }

        public string Choose()
        {
            Chosen = true;
            return Template.ResultsDescription;
        }
    }

    public class PlayerActionChoiceTemplate
    {

        #region properties

        public string Description { get; private set; }

        public ChoiceResults Result { get; private set; }

        public string ResultsDescription { get; private set; }

        public float Value { get; private set; }

        #endregion

        public PlayerActionChoiceTemplate(string description, float value, ChoiceResults results, string resultDescription)
        {
            Value = value;
            Result = results;
            ResultsDescription = resultDescription;
            Description = description;
        }
    }
}