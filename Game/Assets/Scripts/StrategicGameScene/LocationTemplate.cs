using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene
{
    using System.Linq;

    #region EncounterTemplate

    public class EncounterTemplate : IIdentifiable<string>
    {
        private readonly IEnumerable<ChoiceTemplate> r_choices;

        public string Name { get; private set; }

        public string Message { get; private set; }

        public IEnumerable<ChoiceTemplate> Choices
        {
            get
            {
                return this.r_choices == null ? null : this.r_choices.Where(choice => choice.Condition.Passed());
            }
        }

        public EncounterTemplate(string name, string message, IEnumerable<ChoiceTemplate> choices)
        {
            Name = name;
            Message = message;
            if (choices == null)
            {
                return;
            }

            r_choices = choices.Materialize();
        }

        public override string ToString()
        {
            return "{0}, message: {1}, choices: {2}".FormatWith(Name, Message, Choices.ToJoinedString(","));
        }
    }

    #endregion EncounterTemplate

    #region ChoiceTemplate

    public class ChoiceTemplate
    {
        #region properties

        public string Description { get; private set; }

        public ChoiceResult Result { get; private set; }

        public Condition Condition { get; private set; }

        #endregion properties

        public ChoiceTemplate(
            string message,
            ChoiceResult result = null,
            Condition condition = null)
        {
            Result = result;
            Description = message;
            if (condition == null)
            {
                condition = Condition.AlwaysTrue;
            }

            Condition = condition;
        }
    }

    #endregion ChoiceTemplate

    #region Condition

    public class Condition
    {
        #region fields

        private static readonly Condition sr_alwaysTrue = new Condition();

        private static readonly Dictionary<ConditionType, Func<string, double, bool>> sr_conditions =
            new Dictionary<ConditionType, Func<string, double, bool>>
        {
            { ConditionType.None, Succeed },
            { ConditionType.RelationsWith, CheckRelations },
        };

        private readonly string r_key;

        private readonly double r_value;

        private readonly ConditionType r_condition;

        #endregion fields

        public static Condition AlwaysTrue
        {
            get { return sr_alwaysTrue; }
        }

        public Condition(ConditionType condition = ConditionType.None, string key = "", double value = 0)
        {
            r_key = key;
            r_value = value;
            r_condition = condition;
        }

        public bool Passed()
        {
            return sr_conditions.Get(r_condition)(r_key, r_value);
        }

        #region private methods

        private static bool Succeed(string key, double value)
        {
            return true;
        }

        private static bool CheckRelations(string key, double value)
        {
            return GlobalState.Instance.StrategicMap.State.Relations.TryGetOrAdd(key, () => 0) >= value;
        }

        #endregion private methods
    }

    #endregion Condition

    #region ChoiceResult

    public class ChoiceResult
    {
        #region properties

        public string Message { get; private set; }

        public ChoiceResultType Result { get; private set; }

        public string Key { get; private set; }

        public double Value { get; private set; }

        #endregion properties

        public ChoiceResult(string message, ChoiceResultType result = ChoiceResultType.None, string key = "", double value = 0)
        {
            Key = key;
            Value = value;
            Result = result;
            Message = message;
        }
    }

    #endregion ChoiceResult
}