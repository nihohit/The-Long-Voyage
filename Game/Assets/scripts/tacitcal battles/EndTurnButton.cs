public class EndTurnButton : CircularButton {

	// Use this for initialization
	void Start () 
    {
        Action = TacticalState.StartTurn;
	}
}
