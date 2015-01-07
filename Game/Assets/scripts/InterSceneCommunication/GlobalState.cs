namespace Assets.Scripts.InterSceneCommunication
{
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Scripts.LogicBase;
    using Assets.Scripts.Base;

    /// <summary>
    /// A static class, accessible from anywhere in the code, containing all cross-scene information.
    /// </summary>
    public class GlobalState
    {
        public TacticalBattleInformation TacticalBattle { get; set; }

        public StrategicMapInformation StrategicMap { get; private set; }

        public EndBattleSummary BattleSummary { get; set; }

        public bool ActiveGame { get { return StrategicMap != null; } }

        public static GlobalState Instance { get { return Singleton<GlobalState>.Instance; } }

        private GlobalState()
        { }

        public void StartNewGame(string playerName)
        {
            BattleSummary = null;
            TacticalBattle = null;
            StrategicMap = new StrategicMapInformation(playerName);
        }

        public void DefaultInitialization()
        {
            var mechTemplate = EntityTemplateStorage.Instance.GetConfiguration("StandardMech");
            for (int i = 0; i < 2; i++)
            {
                StrategicMap.State.AvailableEntities.Add(new SpecificEntity(mechTemplate));
            }

            mechTemplate = EntityTemplateStorage.Instance.GetConfiguration("ScoutMech");
            for (int i = 0; i < 2; i++)
            {
                StrategicMap.State.AvailableEntities.Add(new SpecificEntity(mechTemplate));
            }

            var systems = SubsystemTemplateStorage.Instance.GetAllConfigurations().ToArray();
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    StrategicMap.State.AvailableSystems.Add(systems[i]);
                }
            }
        }

        public void EndGame()
        {
            TacticalBattle = null;
            StrategicMap = null;
            BattleSummary = null;
        }
    }
}