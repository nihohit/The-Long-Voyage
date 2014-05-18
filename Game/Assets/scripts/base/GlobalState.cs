using UnityEngine;
using System.Collections.Generic;

public static class GlobalState
{
    public static int AmountOfHexes { get; set; }

    public static IEnumerable<ActiveEntity> EntitiesInBattle { get; set; }
}
