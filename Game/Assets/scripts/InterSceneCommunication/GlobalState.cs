using Assets.scripts.TacticalBattleScene;
using System.Collections.Generic;

namespace Assets.scripts.InterSceneCommunication
{
    public static class GlobalState
    {
        public static TacticalBattleInformation TacticalBattle { get; set; }

        public static StrategicMapInformation StrategicMap { get; set; }

        public static EndBattleSummary BattleSummary { get; set; }
    }
}