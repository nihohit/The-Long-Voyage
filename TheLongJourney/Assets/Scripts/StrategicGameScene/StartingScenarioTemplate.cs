using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene
{
    using Base;
    using LogicBase;

    #region StartingConditions

    public class StartingScenarioTemplate : IIdentifiable<string>
    {
        #region properties

        public string Name { get; private set; }

        public IEnumerable<EquippedEntity> PlayerMechs { get; private set; }

        #endregion properties

        #region constructors

        public StartingScenarioTemplate(IEnumerable<EquippedEntity> mechs, string name)
        {
            PlayerMechs = mechs;
            Name = name;
        }

        #endregion constructors
    }

    #endregion StartingConditions
}