using System;

namespace Assets.Scripts.LogicBase
{
    //the logic behind the numbering is that the addition of this enumerator and MovementType gives the following result - if the value is between 0-5, no movement penalty. above 4 - slow, above 6 - impossible
    public enum MovementType { Crawler = 3, Walker = 2, Hover = 1, Flyer = 0, Unmoving = Int32.MaxValue }

    [Flags]
    public enum VisualProperties
    {
        None = 0,
        AppearsOnRadar = 1,
        AppearsOnSight = 2,
        BlocksSight = 4,
    }

    // defines what kind of entities can a system target
    [Flags]
    public enum TargetingType
    {
        Enemy = 1,
        Friendly = 2,
        AllEntities = 3,
        AllHexes = 4
    }

    // Variants of mech designs
    public enum EntityVariant
    { Regular }

    // the way a system reaches its targets
    public enum DeliveryMethod { Direct, Unobstructed }

    // The possible effects of a system
    public enum EffectType { EmpDamage, HeatDamage, IncendiaryDamage, PhysicalDamage, FlameHex }

    //to be filled with all different factions
    public enum Loyalty { Player, EnemyArmy, Monsters, Bandits, Inactive, Friendly }
}