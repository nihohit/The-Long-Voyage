using Assets.Scripts.Base;
using System.Collections.Generic;

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

        public ChoiceTemplate(string message, string resultMessage, ChoiceResults results = ChoiceResults.None)
        {
            Result = results;
            ResultsDescription = resultMessage;
            Description = message;
        }
    }
}