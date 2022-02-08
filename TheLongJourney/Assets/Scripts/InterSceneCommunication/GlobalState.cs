namespace Assets.Scripts.InterSceneCommunication {
  using Assets.Scripts.Base;
  using Assets.Scripts.LogicBase;
  using System.Linq;

  /// <summary>
  /// A static class, accessible from anywhere in the code, containing all cross-scene information.
  /// </summary>
  public class GlobalState {
    public static GlobalState Instance {
      get {
        return Singleton<GlobalState>.Instance;
      }
    }

    public TacticalBattleInformation TacticalBattle { get; set; }

    public StrategicMapInformation StrategicMap { get; private set; }

    public EndBattleSummary BattleSummary { get; set; }

    public Configurations Configurations { get; private set; }

    public bool ActiveGame {
      get {
        return StrategicMap != null;
      }
    }

    private GlobalState() {
      Configurations = new Configurations();
    }

    public void StartNewGame(string playerName) {
      BattleSummary = null;
      TacticalBattle = null;
      StrategicMap = new StrategicMapInformation(playerName);
    }

    public void EndGame() {
      TacticalBattle = null;
      StrategicMap = null;
      BattleSummary = null;
    }
  }
}