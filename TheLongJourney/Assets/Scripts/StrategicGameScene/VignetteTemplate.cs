using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene {
  using System.Linq;

  #region VignetteTemplate

  public class VignetteTemplate : IIdentifiable<string> {
    public bool IsFirstPart { get; private set; }

    public EventResult Result { get; private set; }

    public string Name { get; private set; }

    public string Message { get; private set; }

    public IEnumerable<ChoiceTemplate> Choices { get; private set; }

    public VignetteTemplate(string name, string message, IEnumerable<ChoiceTemplate> choices = null,
      EventResult result = null, bool starting = false) {
      Name = name;
      Message = message;
      Choices = choices == null ? new List<ChoiceTemplate>() : choices.ToList();
      Result = result;
      IsFirstPart = starting;
    }

    public override string ToString() {
      return "{0}, message: {1}, choices: {2}".FormatWith(Name, Message,
        Choices == null ? "none." : Choices.Select(choice => choice.Description).ToJoinedString());
    }
  }

  #endregion VignetteTemplate

  #region ChoiceTemplate

  public class ChoiceTemplate {
    WeightedChoices<string> resultOptions;

    #region properties

    public string Description { get; private set; }

    public VignetteTemplate Result {
      get {
        return GlobalState.Instance.Configurations.Vignettes
          .GetConfiguration(Randomiser.ChooseWeightedValues(resultOptions, 1).First());
      }
    }

    public Condition Condition { get; private set; }

    #endregion properties

    public ChoiceTemplate(
      string message,
      string nextEvent = null,
      Condition condition = null,
      IEnumerable<ObjectChancePair<string>> nextEventOptions = null) {
      Description = message;
      Assert.AssertConditionMet(!nextEvent.IsNullOrEmpty() || !nextEventOptions.IsNullOrEmpty(),
        "Either nextEvent or nextEventOptions shouldn't be null");
      Assert.AssertConditionMet(nextEvent.IsNullOrEmpty() || nextEventOptions.IsNullOrEmpty(),
        "nextEvent or nextEventOptions should be null");
      this.resultOptions = !nextEventOptions.IsNullOrEmpty() ? new WeightedChoices<string>(nextEventOptions) :
        new WeightedChoices<string>(new Dictionary<string, double> { { nextEvent, 1.0 } });

      if (condition == null) {
        condition = Condition.AlwaysTrue;
      }

      Condition = condition;
    }
  }

  #endregion ChoiceTemplate

  #region Condition

  public class Condition {
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

    public static Condition AlwaysTrue {
      get { return sr_alwaysTrue; }
    }

    public Condition(ConditionType condition = ConditionType.None, string key = "", double value = 0) {
      r_key = key;
      r_value = value;
      r_condition = condition;
    }

    public bool Available() {
      return sr_conditions.Get(r_condition)(r_key, r_value);
    }

    #region private methods

    private static bool Succeed(string key, double value) {
      return true;
    }

    private static bool CheckRelations(string key, double value) {
      return GlobalState.Instance.StrategicMap.State.Relations.TryGetOrAdd(key, () => 0) >= value;
    }

    #endregion private methods
  }

  #endregion Condition

  #region EventResult

  public class EventResult {
    #region properties

    public EventResultType ResultType { get; private set; }

    public string Key { get; private set; }

    public double Value { get; private set; }

    #endregion properties

    public EventResult(EventResultType result = EventResultType.None, string key = "", double value = 0) {
      Key = key;
      Value = value;
      ResultType = result;

      Assert.AssertConditionMet(!result.HasFlag(EventResultType.AffectRelations) || (!string.IsNullOrEmpty(key) && value != 0), "key or value missing from choice result");
    }
  }

  #endregion EventResult
}