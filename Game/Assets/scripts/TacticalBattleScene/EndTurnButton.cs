using Assets.scripts.UnityBase;

namespace Assets.scripts.TacticalBattleScene
{
    public class EndTurnButton : CircularButton
    {
        // Use this for initialization
        private void Start()
        {
            ClickableAction = TacticalState.StartTurn;
        }
    }
}