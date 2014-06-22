using Assets.scripts.TacticalBattleScene;
using System.Collections.Generic;

namespace Assets.scripts.LogicBase
{
    public static class GlobalState
    {
        public static int AmountOfHexes { get; set; }

        public static IEnumerable<ActiveEntity> EntitiesInBattle { get; set; }
    }
}