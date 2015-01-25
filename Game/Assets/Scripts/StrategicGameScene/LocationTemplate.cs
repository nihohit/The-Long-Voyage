using Assets.Scripts.Base;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.StrategicGameScene
{
    public class EncounterTemplate : IIdentifiable<string>
    {
        public string Name { get; private set; }

        public string Message { get; private set; }

        public IEnumerable<ChoiceTemplate> Choices { get; private set; }

        public EncounterTemplate(string name, string message, IEnumerable<ChoiceTemplate> choices)
        {
            Name = name;
            Message = message;
            if (choices == null)
            {
                return;
            }

            Choices = choices.Materialize();
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

    public sealed class LocationTemplateConfigurationStorage : ConfigurationStorage<EncounterTemplate, LocationTemplateConfigurationStorage>
    {
        private LocationTemplateConfigurationStorage()
            : base("Locations")
        {
        }

        protected override JSONParser<EncounterTemplate> GetParser()
        {
            return new LocationTemplateJsonParser();
        }

        private class LocationTemplateJsonParser : JSONParser<EncounterTemplate>
        {
            private readonly ChoiceTemplateJsonParser r_choiceParser = new ChoiceTemplateJsonParser();

            protected override EncounterTemplate ConvertCurrentItemToObject()
            {
                return new EncounterTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<string>("Description"),
                    GetChoices(TryGetValueOrSetDefaultValue<IEnumerable<object>>("Choices", null)).Materialize());
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
                    yield return this.r_choiceParser.ConvertToObject(choiceAsDictionary);
                }
            }

            private class ChoiceTemplateJsonParser : JSONParser<ChoiceTemplate>
            {
                protected override ChoiceTemplate ConvertCurrentItemToObject()
                {
                    return new ChoiceTemplate(
                        TryGetValueAndFail<string>("Description"),
                        TryGetValueOrSetDefaultValue("Result", ChoiceResults.None),
                        TryGetValueAndFail<string>("ResultsDescription"));
                }
            }
        }
    }
}