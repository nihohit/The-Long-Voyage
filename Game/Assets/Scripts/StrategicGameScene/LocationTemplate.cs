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

        public ChoiceTemplate(string description, string resultDescription, ChoiceResults results = ChoiceResults.None)
        {
            Result = results;
            ResultsDescription = resultDescription;
            Description = description;
        }
    }

    public sealed class LocationTemplateConfigurationStorage : ConfigurationStorage<EncounterTemplate, LocationTemplateConfigurationStorage>
    {
        private LocationTemplateConfigurationStorage()
            : base("Locations", new LocationTemplateJsonParser())
        {
        }

        private class LocationTemplateJsonParser : JSONParser<EncounterTemplate>
        {
            protected override EncounterTemplate ConvertCurrentItemToObject()
            {
                return new EncounterTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<string>("Description"),
                    GetChoices(TryGetValueOrSetDefaultValue<IEnumerable<object>>("Choices", null)).Materialize());
            }

            private IEnumerable<ChoiceTemplate> GetChoices(IEnumerable<object> choices)
            {
                if (choices == null)
                {
                    return null;
                }

                return this.ParseChoices(choices);
            }

            private IEnumerable<ChoiceTemplate> ParseChoices(IEnumerable<object> choices)
            {
                foreach (var choiceAsDictionary in choices.Select(choice => choice as Dictionary<string, object>))
                {
                    Assert.NotNull(choiceAsDictionary, "A certain choice isn't a dictionary.");
                    yield return ObjectConstructor.ParseObject<ChoiceTemplate>(choiceAsDictionary);

                    // this.r_choiceParser.ConvertToObject(choiceAsDictionary);
                }
            }
        }
    }
}