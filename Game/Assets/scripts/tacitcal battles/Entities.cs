using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#region Entity

public abstract class Entity
{
    #region private fields

    private static int s_idCounter = 0;

    private readonly int m_id;

    #endregion

    #region constructor

    public Entity(EntityType type, Loyalty loyalty, double health, double shield, VisualProperties visuals, EntityReactor reactor)
    {
        UnitType = type;
        Loyalty = loyalty;
        Health = health;
        Shield = shield;
        Visuals = visuals;
        reactor.Entity = this;
        this.Marker = reactor;
        m_id = s_idCounter++;
    }

    #endregion

    #region properties

    public int ID { get {return m_id;}}

    public MarkerScript Marker { get; set; }

    public EntityType UnitType { get; private set; }

    public double Health { get; private set; }

    public double Shield { get; private set; }

    public VisualProperties Visuals { get; private set; }

    public virtual Hex Hex { get; set; }

    public Loyalty Loyalty { get; private set; }

    #endregion

    #region public methods

    public virtual void Hit(double damage, DamageType damageType)
    {
        Debug.Log("{0} was hit for damage {1} and type {2}".FormatWith(this, damage, damageType));
        //TODO - handle damage types
        Health -= damage;
        Debug.Log("{0} has now {1} health and {2} shields".FormatWith(this, Health, Shield));
        if(Health <= 0)
        {
            Destroy();
        }
    }

    #region object overrides

    public override bool Equals(object obj)
    {
        var ent = obj as Entity;
        return ent != null &&
            UnitType == ent.UnitType &&
            ID == ent.ID;
    }

    public override int GetHashCode()
    {
        return Hasher.GetHashCode(UnitType, Marker, m_id);
    }

    public override string ToString()
    {
        return "{0}, Id={1}, loyalty ={5}, health={2}, shield={3}, Visuals={4}".FormatWith(UnitType, m_id, Health, Shield, Visuals, Loyalty);
    }

    #endregion
    #endregion

    #region private methods

    private void Destroy()
    {
        Debug.Log("Destroy {0}".FormatWith(this));
        this.Hex.Content = null;
        UnityEngine.Object.Destroy(this.Marker.gameObject);
    }

    #endregion
}

#endregion

#region inanimate entities

public abstract class TerrainEntity : Entity
{
    private Hex m_hex;

    public TerrainEntity(double health, bool visibleOnRadar, EntityReactor reactor)
        : base(EntityType.TerrainFeature, Loyalty.Neutral, health, 0, 
               VisualProperties.AppearsOnSight | (visibleOnRadar ? VisualProperties.AppearsOnRadar : VisualProperties.None), reactor)
    {}

    public override Hex Hex { 
        get
        {
            return m_hex;
        }
        set
        {
            m_hex = value;
            Assert.AreEqual(m_hex.Conditions, TraversalConditions.Broken, "terrain entities are always placed over broken land to ensure that when they're destroyed there's rubble below");
        }
    }
}

public class DenseTrees : TerrainEntity
{
    public DenseTrees(EntityReactor reactor) : base(FileHandler.GetIntProperty("Dense trees health", FileAccessor.Units), false, reactor)
    { }
}

public class SparseTrees : TerrainEntity
{
    public SparseTrees(EntityReactor reactor) : base(FileHandler.GetIntProperty("Sparse trees health", FileAccessor.Units), false, reactor)
    { }
}

public class Building : TerrainEntity
{
    public Building(EntityReactor reactor) : base(FileHandler.GetIntProperty("Building health", FileAccessor.Units), false, reactor)
    { }
}

#endregion

#region ActiveEntity

public abstract class ActiveEntity : Entity
{
    public ActiveEntity(Loyalty loyalty, double radarRange, double sightRange, IEnumerable<Subsystem> systems, EntityType type, double health, double shield, VisualProperties visuals, EntityReactor reactor) :
        base(type, loyalty, health, shield, visuals, reactor)
    {
        RadarRange = radarRange;
        SightRange = sightRange;
        Systems = systems;
    }

    public double RadarRange { get; private set; }

    public double SightRange { get; private set; }

    public IEnumerable<Subsystem> Systems { get; private set; }

    public virtual IEnumerable<PotentialAction> ComputeActions()
    {
        var dict = new Dictionary<Hex, List<PotentialAction>>();
        return Systems.Where(system => system.Operational()).SelectMany(system => system.ActionsInRange(this.Hex, dict));
    }
}

#endregion

#region MovingEntity

public abstract class MovingEntity : ActiveEntity
{
    public MovingEntity(MovementType movement, double speed, Loyalty loyalty, double radarRange, double sightRange, IEnumerable<Subsystem> systems, EntityType type, double health, double shield, VisualProperties visuals, EntityReactor reactor) :
        base(loyalty, radarRange, sightRange, systems, type, health, shield, visuals, reactor)
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
            movement.ActingEntity = this;
        }
        return baseActions.Union(possibleHexes.Values.Select(movement => (PotentialAction)movement));
    }
}

#endregion

#region Mech
//TODO - should be replaced with XML configuration files

public class Mech : MovingEntity
{
    public Mech(IEnumerable<Subsystem> systems, EntityReactor reactor,
                double health = 5,
                double shield = 3,
                VisualProperties visuals = VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight,
                double speed = 4,
                Loyalty loyalty = Loyalty.Player,
                double radarRange = 20,
                double sightRange = 10) :
        base(MovementType.Walker, speed, loyalty, radarRange, sightRange, systems, EntityType.Mech, health, shield, visuals, reactor)
    { }
}

#endregion