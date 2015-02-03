using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene
{
    using Assets.Scripts.Base;
    using Assets.Scripts.InterSceneCommunication;
    using Assets.Scripts.LogicBase;

    #region StartingConditions

    public class ScenarioTemplate : IIdentifiable<string>
    {
        #region properties

        public string Name { get; private set; }

        public IEnumerable<EquippedEntity> Mechs { get; private set; }

        public EncounterTemplate Encounter { get; private set; }

        #endregion properties

        #region constructors

        public ScenarioTemplate(string encounter, IEnumerable<EquippedEntity> mechs, string name)
        {
            Encounter = GlobalState.Instance.Configurations.Encounters.GetConfiguration(encounter);
            Mechs = mechs;
            Name = name;
        }

        #endregion constructors
    }

    #endregion StartingConditions
}