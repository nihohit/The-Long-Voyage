using Assets.Scripts.Base;

using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.StrategicGameScene
{
    public class LocationTemplate : IIdentifiable<string>
    {
        public string Name { get; private set; }

        public string Message { get; private set; }

        public IEnumerable<ChoiceTemplate> Choices { get; private set; }

        public LocationTemplate(string name, string message, IEnumerable<ChoiceTemplate> choices)
        {
            Name = name;
            Message = message;
            if (choices == null)
            {
                return;
            }

            Choices = choices.Materialize();
            const int maxChoices = 4;
            Assert.EqualOrLesser(Choices.Count(), maxChoices,
                "Too many choices in LocationScript {0}. Maximum allowed is {1}".FormatWith(Message, maxChoices));
        }
    }

    public class PlayerActionChoice
    {
        public ChoiceTemplate Template { get; private set; }

        public PlayerActionChoice(ChoiceTemplate template)
        {
            Template = template;
        }

        public string Choose()
        {
            return Template.ResultsDescription;
        }
    }

    public class ChoiceTemplate
    {
        #region properties

        public string Description { get; private set; }

        public ChoiceResults Result { get; private set; }

        public string ResultsDescription { get; private set; }

        #endregion properties

        public ChoiceTemplate(string description, ChoiceResults results, string resultDescription)
        {
            Result = results;
            ResultsDescription = resultDescription;
            Description = description;
        }
    }

    public sealed class LocationTemplateConfigurationStorage : ConfigurationStorage<LocationTemplate, LocationTemplateConfigurationStorage>
    {
        private LocationTemplateConfigurationStorage()
            : base("Locations")
        { }

        protected override JSONParser<LocationTemplate> GetParser()
        {
            return new LocationTemplateJSONParser();
        }

        private class LocationTemplateJSONParser : JSONParser<LocationTemplate>
        {
            private ChoiceTemplateJSONParser m_choiceParser = new ChoiceTemplateJSONParser();

            protected override LocationTemplate ConvertCurrentItemToObject()
            {
                return new LocationTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<string>("Description"),
                    GetChoices(TryGetValueOrSetDefaultValue<IEnumerable<object>>("Choices", null)).Materialize()
                    );
            }

            private IEnumerable<ChoiceTemplate> GetChoices(object unParsedChoices)
            {
                if (unParsedChoices == null)
                {
                    return null;
                }

                var choices = unParsedChoices as IEnumerable<object>;

                Assert.NotNull(choices, "'Choices' part in location isn't an array.");

                return this.ParseChoices(choices);
            }

            private IEnumerable<ChoiceTemplate> ParseChoices(IEnumerable<object> choices)
            {
                foreach (var choiceAsDictionary in choices.Select(choice => choice as Dictionary<string, object>))
                {
                    Assert.NotNull(choiceAsDictionary, "A certain choice isn't a dictionary.");
                    yield return this.m_choiceParser.ConvertToObject(choiceAsDictionary);
                }
            }

            private class ChoiceTemplateJSONParser : JSONParser<ChoiceTemplate>
            {
                protected override ChoiceTemplate ConvertCurrentItemToObject()
                {
                    return new ChoiceTemplate(
                        TryGetValueAndFail<string>("Description"),
                        TryGetValueOrSetDefaultValue<ChoiceResults>("Result", ChoiceResults.None),
                        TryGetValueAndFail<string>("ResultsDescription")
                        );
                }
            }
        }
    }
}