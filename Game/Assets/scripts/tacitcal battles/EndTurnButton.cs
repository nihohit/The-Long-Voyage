public class EndTurnButton : CircularButton
{
    // Use this for initialization
    private void Start()
    {
        Action = TacticalState.StartTurn;
    }
}