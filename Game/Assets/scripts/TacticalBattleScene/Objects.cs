using System;

namespace Assets.Scripts.TacticalBattleScene
{
    #region enums

    public enum Biome { Tundra, City, Grass, Desert, Swamp }

    [Flags]
    public enum HexEffect
    {
        None = 0,
        Heating = 1,
        Chilling = 2,
    }

    //the logic behind the numbering is that the addition of this enumerator and MovementType gives the following result - if the value is between 0-5, no movement penalty. above 4 - slow, above 6 - impossible
    public enum TraversalConditions
    {
        Easy = 0,
        Uneven = 1, //hard to crawl, everything else is fine
        Broken = 2, //hard to crawl or walk, everything else is fine
        NoLand = 4, //can't crawl or walk, everything else is fine
        Blocked = 5 //can only fly
    }

    // there needs to be an order of importance - the more severe damage has a higher value
    public enum SystemCondition { Operational = 0, OutOfAmmo = 1, Neutralized = 2, Destroyed = 3 }

    #endregion enums

    #region delegates

    public delegate bool EntityCheck(EntityReactor ent);

    public delegate void HexOperation(HexReactor hex);

    public delegate double HexTraversalCost(HexReactor hex);

    public delegate bool HexCheck(HexReactor hex);

    #endregion delegates
}