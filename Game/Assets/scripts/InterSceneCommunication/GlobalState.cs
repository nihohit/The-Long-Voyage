namespace Assets.Scripts.InterSceneCommunication
{
    /// <summary>
    /// A static class, accessible from anywhere in the code, containing all cross-scene information.
    /// </summary>
    public static class GlobalState
    {
        public static TacticalBattleInformation TacticalBattle { get; set; }

        public static StrategicMapInformation StrategicMap { get; set; }

        public static EndBattleSummary BattleSummary { get; set; }

        public static void Init()
        {
        }
    }
}