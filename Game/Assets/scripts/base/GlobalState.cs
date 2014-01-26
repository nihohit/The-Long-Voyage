using UnityEngine;
using System.Collections;

public class GlobalState : Singleton<GlobalState>
{
	private static GlobalState s_instance;

    public int AmountOfHexes { get; set; }

    private GlobalState() { }
}
