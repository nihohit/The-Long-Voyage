using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#region Entity

public abstract class Entity
{
    public Entity(EntityType type, double health, double shield, VisualProperties visuals)
    {
        Type = type;
        Health = health;
        Shield = shield;
        Visuals = visuals;
    }

    public MarkerScript Marker { get; set; }

    public EntityType Type { get; private set; }

    public double Health { get; private set; }

    public double Shield { get; private set; }

    public VisualProperties Visuals { get; private set; }

    public Hex Hex { get; set; }
}

#endregion

#region ActiveEntity

public abstract class ActiveEntity : Entity
{
    public ActiveEntity(Loyalty loyalty, double radarRange, double sightRange, IEnumerable<Subsystem> systems, EntityType type, double health, double shield, VisualProperties visuals) :
        base(type, health, shield, visuals)
    {
        RadarRange = radarRange;
        SightRange = sightRange;
        Systems = systems;
        Loyalty = Loyalty;
    }

    public Loyalty Loyalty { get; private set; }

    public double RadarRange { get; private set; }

    public double SightRange { get; private set; }

    public IEnumerable<Subsystem> Systems { get; private set; }

    public virtual IEnumerable<PotentialAction> ComputeActions()
    {
        var result = new List<PotentialAction>();
        //TODO - check for subsystems
        return result;
    }
}

#endregion

#region MovingEntity

public abstract class MovingEntity : ActiveEntity
{
    public MovingEntity(MovementType movement, double speed, Loyalty loyalty, double radarRange, double sightRange, IEnumerable<Subsystem> systems, EntityType type, double health, double shield, VisualProperties visuals) :
        base(loyalty, radarRange, sightRange, systems, type, health, shield, visuals)
    {
        Speed = speed;
        Movement = movement;
    }

    public double Speed { get; private set; }

    public MovementType Movement { get; private set; }

    public override IEnumerable<PotentialAction> ComputeActions()
    {
        var baseActions = base.ComputeActions();
        var possibleHexes = AStar.FindAllAvailableHexes(this.Hex, this.Speed, this.Movement);
        foreach (var movement in possibleHexes.Values)
        {
            movement.Entity = this;
        }
        return baseActions.Union(possibleHexes.Values.Select(movement => (PotentialAction)movement));
    }
}

#endregion

#region Mech
//TODO - should be replaced with XML configuration files

public class Mech : MovingEntity
{
    public Mech(IEnumerable<Subsystem> systems,
                double health = 5,
                double shield = 3,
                VisualProperties visuals = VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight,
                double speed = 4,
                Loyalty loyalty = Loyalty.Player,
                double radarRange = 20,
                double sightRange = 10) :
        base(MovementType.Walker, speed, loyalty, radarRange, sightRange, systems, EntityType.Mech, health, shield, visuals)
    { }
}

#endregion
