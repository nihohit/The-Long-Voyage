using Assets.Scripts.UnityBase;

namespace Assets.Scripts.TacticalBattleScene {
  public class EndTurnButton : SimpleButton {
    // Use this for initialization
    private void Start() {
      ClickableAction = TacticalState.StartTurn;
    }
  }
}