using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Entity

public abstract class Entity
{
    #region private fields

    private static int s_idCounter = 0;

    private readonly int m_id;

    private readonly String m_name;

    private readonly double m_armor;

    #endregion private fields

    #region constructor

    public Entity(Loyalty loyalty, double health, VisualProperties visuals, EntityReactor reactor)
    {
        Loyalty = loyalty;
        Health = health;
        Visuals = visuals;
        reactor.Entity = this;
        this.Marker = reactor;
        m_id = s_idCounter++;
        m_name = "{0} {1} {2}".FormatWith(this.GetType().ToString(), Loyalty, m_id);
        if((this.Visuals & VisualProperties.AppearsOnRadar) != 0)
        {
            TacticalState.AddRadarVisibleEntity(this);
        }
    }

    #endregion constructor

    #region properties

    public int ID { get { return m_id; } }

    public MarkerScript Marker { get; private set; }

    public double Health { get; private set; }

    public VisualProperties Visuals { get; private set; }

    public virtual Hex Hex { get; set; }

    public Loyalty Loyalty { get; private set; }

    public String Name { get { return m_name; } }

    #endregion properties

    #region public methods

    public void Affect(double strength, EffectType effectType)
    {
        Debug.Log("{0} was hit for damage {1} and type {2}".FormatWith(m_name, strength, effectType));
        var remainingDamage = ExternalDamage(strength, effectType);
        InternalDamage(remainingDamage, effectType);
        Debug.Log("{0} is now in state {1}".FormatWith(m_name, FullState()));

        if (Destroyed())
        {
            Destroy();
        }
    }

    // this function returns a string value that represents the mutable state of the entity
    public virtual string FullState()
    {
        return "Health {0} Hex {1}".FormatWith(Health, Hex);
    }

    // just a simple function to make the code more readable
    public virtual bool Destroyed()
    {
        return Health <= 0;
    }

    #region object overrides

    public override bool Equals(object obj)
    {
        var ent = obj as Entity;
        return ent != null &&
            ID == ent.ID;
    }

    public override int GetHashCode()
    {
        return Hasher.GetHashCode(m_name, Marker, m_id);
    }

    public override string ToString()
    {
        return m_name;
    }

    #endregion object overrides

    #endregion public methods

    #region protected methods

    protected virtual void InternalDamage(double damage, EffectType damageType)
    {
        switch (damageType)
        {
            case EffectType.PhysicalDamage:
                Health -= damage;
                break;

            case EffectType.IncendiaryDamage:
                Health -= damage/2;
                break;
        }
    }

    protected virtual double ExternalDamage(double strength, EffectType damageType)
    {
        switch (damageType)
        {
            case EffectType.PhysicalDamage:
            case EffectType.IncendiaryDamage:
                return strength - m_armor;
                
            case EffectType.EmpDamage:
            case EffectType.HeatDamage:
                return strength;


            default:
                throw new UnknownTypeException(damageType);
        }
    }

    protected virtual void Destroy()
    {
        Debug.Log("Destroy {0}".FormatWith(m_name));
        this.Hex.Content = null;
        TacticalState.DestroyEntity(this);
        UnityEngine.Object.Destroy(this.Marker.gameObject);
    }

    #endregion protected methods
}

#endregion Entity

#region inanimate entities

public abstract class TerrainEntity : Entity
{
    private Hex m_hex;

    public TerrainEntity(double health, bool visibleOnRadar, bool blocksSight, EntityReactor reactor)
        : base(Loyalty.Inactive, health, 
               VisualProperties.AppearsOnSight | (visibleOnRadar ? VisualProperties.AppearsOnRadar : VisualProperties.None) | (blocksSight ? VisualProperties.BlocksSight : VisualProperties.None), reactor)
    { }

    public override Hex Hex
    {
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

    //inanimate objects take heat damage as physical damage
    protected override void InternalDamage(double damage, EffectType damageType)
    {
        if (damageType == EffectType.HeatDamage || damageType == EffectType.IncendiaryDamage)
        {
            damageType = EffectType.PhysicalDamage;
        }
        base.InternalDamage(damage, damageType);
    }
}

public class DenseTrees : TerrainEntity
{
    public DenseTrees(EntityReactor reactor)
        : base(FileHandler.GetIntProperty("Dense trees health", FileAccessor.Units), false, true, reactor)
    { }
}

public class SparseTrees : TerrainEntity
{
    public SparseTrees(EntityReactor reactor)
        : base(FileHandler.GetIntProperty("Sparse trees health", FileAccessor.Units), false, false, reactor)
    { }
}

public class Building : TerrainEntity
{
    public Building(EntityReactor reactor)
        : base(FileHandler.GetIntProperty("Building health", FileAccessor.Units), true, true, reactor)
    { }
}

#endregion inanimate entities

#region ActiveEntity

public abstract class ActiveEntity : Entity
{
    #region constructor

    public ActiveEntity(double maximumEnergy, Loyalty loyalty, int radarRange, int sightRange, IEnumerable<Subsystem> systems, double health, double shield, VisualProperties visuals, EntityReactor reactor) :
        base(loyalty, health, visuals, reactor)
    {
        m_radarRange = radarRange;
        m_sightRange = sightRange;
        m_systems = systems;
        m_maxEnergy = maximumEnergy;
        CurrentEnergy = maximumEnergy;
        m_tempMaxEnergy = maximumEnergy;
        m_maxShields = shield;
        Shield = shield;
        //TODO - add those variables
        m_shieldRechargeRate = 1;
        m_maxHeat = 5;
    }

    #endregion constructor

    #region private fields

    private HashSet<Hex> m_detectedHexes;

    private readonly int m_radarRange, m_sightRange;

    private readonly IEnumerable<Subsystem> m_systems;

    private IEnumerable<PotentialAction> m_actions;

    private readonly double m_maxEnergy, m_maxHeat, m_maxShields, m_heatLossRate, m_shieldRechargeRate;

    private double m_tempMaxEnergy;

    private bool m_wasShutDown = false;

    #endregion private fields

    #region Properties

    public IEnumerable<PotentialAction> Actions
    {
        get
        {
            if (m_actions == null)
            {
                m_actions = ComputeActions().Materialize();
            }
            return m_actions;
        }
    }

    public double CurrentEnergy { get; set; }

    public HashSet<Hex> SeenHexes { get; private set; }

    public double Shield { get; private set; }

    public double Heat { get; private set; }

    #endregion Properties

    #region public methods

    public void ResetActions()
    {
        //Debug.Log("{0} is resetting actions".FormatWith(Name));
        if (m_actions != null)
        {
            foreach (var action in m_actions)
            {
                action.Destroy();
            }
        }
        m_actions = null;
    }

    public void SetSeenHexes()
    {
        var whatTheEntitySeesNow = FindSeenHexes();
        var whatTheEntitySeesNowInRadar = FindRadarHexes().Except(whatTheEntitySeesNow);
        //Debug.Log("{0} is setting seen hexes".FormatWith(Name));

        if (SeenHexes != null)
        {
            var whatTheEntitySeesNowSet = new HashSet<Hex>(whatTheEntitySeesNow);
            var whatTheEntitySeesNowInRadarSet = new HashSet<Hex>(whatTheEntitySeesNowInRadar);
            //TODO - we can remove this optimization and just call ResetSeenHexes before this, but it'll be more expensive. Until it'll cause problem I'm keeping this
            //this leaves in each list the hexes not in the other
            whatTheEntitySeesNowSet.ExceptOnBoth(SeenHexes);
            whatTheEntitySeesNowInRadarSet.ExceptOnBoth(m_detectedHexes);

            foreach (var hex in SeenHexes)
            {
                hex.Unseen();
            }
            foreach (var hex in m_detectedHexes)
            {
                hex.Undetected();
            }
            foreach (var hex in whatTheEntitySeesNowSet)
            {
                hex.Seen();
            }
            foreach (var hex in whatTheEntitySeesNowInRadarSet)
            {
                hex.Detected();
            }
        }
        else
        {
            foreach (var hex in whatTheEntitySeesNow)
            {
                hex.Seen();
            }
            foreach (var hex in whatTheEntitySeesNowInRadar)
            {
                hex.Detected();
            }
        }
        SeenHexes = new HashSet<Hex>(whatTheEntitySeesNow);
        m_detectedHexes = new HashSet<Hex>(whatTheEntitySeesNowInRadar);
    }

    public void ResetSeenHexes()
    {
        SeenHexes = null;
        m_detectedHexes = null;
        SetSeenHexes();
    }

    public virtual bool StartTurn()
    {
        Debug.Log(FullState());
        ResetActions();
        ResetSeenHexes();
        CurrentEnergy = m_tempMaxEnergy;
        Heat = Math.Max(Heat - m_heatLossRate, 0);
        if (m_tempMaxEnergy <= 0 || Heat >= m_maxHeat)
        {
            if (Heat >= m_maxHeat) Heat = 0;
            m_tempMaxEnergy = m_maxEnergy;
            m_wasShutDown = true;
            return false;
        }
        m_wasShutDown = false;
        m_tempMaxEnergy = m_maxEnergy;
        Shield = Math.Min(m_maxShields, Shield + m_shieldRechargeRate);
        return true;
    }

    public override string FullState()
    {
        return "Shields {5}/{6} Heat {3}/{4} Energy {1}/{2} {0} ".FormatWith(base.FullState(), CurrentEnergy, m_maxEnergy, Heat, m_maxHeat, Shield, m_maxShields);
    }

    public override bool Destroyed()
    {
        return base.Destroyed() || m_systems.None(system => system.Operational()) || ((m_tempMaxEnergy <= 0 || Heat >= m_maxHeat) && m_wasShutDown);
    }

    public bool ShutDown()
    {
        return m_wasShutDown;
    }

    #endregion public methods

    #region private and protected methods

    private IEnumerable<Hex> FindSeenHexes()
    {
        //TODO - we might be able to make this somewhat more efficient by combining the sight & radar raycasts, but we should first make sure that it is needed.
        return Hex.RaycastAndResolve<HexReactor>(0, m_sightRange, (hex) => true, true, (hex) => (hex.Content != null && ((hex.Content.Visuals & VisualProperties.BlocksSight) != 0)), "Hexes", (reactor) => reactor.MarkedHex);
    }

    private IEnumerable<Hex> FindRadarHexes()
    {
        var inactiveRadarVisibleEntityMarkers = TacticalState.RadarVisibleEntities.Where(ent => !ent.Marker.enabled).Select(ent => ent.Marker);
        inactiveRadarVisibleEntityMarkers.ForEach(marker => marker.GetComponent<Collider2D>().enabled = true);
        var results = Hex.RaycastAndResolve(0, m_radarRange, (hex) => hex.Content != null, true, "Entities");
        inactiveRadarVisibleEntityMarkers.ForEach(marker => marker.GetComponent<Collider2D>().enabled = false);
        return results;
    }

    protected override void InternalDamage(double damage, EffectType damageType)
    {
        var heatDamage = 0.0;
        var physicalDamage = 0.0;
        var energyDamage = 0.0;

        switch (damageType)
        {
            case EffectType.IncendiaryDamage:
                heatDamage = damage / 2;
                physicalDamage = damage / 2;
                break;

            case EffectType.PhysicalDamage:
                physicalDamage = damage;
                break;

            case EffectType.EmpDamage:
                energyDamage = damage;
                break;

            case EffectType.HeatDamage:
                heatDamage = damage;
                break;
        }

        Heat += heatDamage;
        m_tempMaxEnergy -= energyDamage;

        if (m_systems.Any(system => system.Operational()))
        {
            m_systems.Where(system => system.Operational()).ChooseRandomValue().Hit(damageType, physicalDamage + energyDamage);
        }

        base.InternalDamage(physicalDamage, EffectType.PhysicalDamage);
    }

    protected virtual IEnumerable<PotentialAction> ComputeActions()
    {
        Debug.Log("{0} is computing actions. Its condition is {1}".FormatWith(this, FullState()));
        var dict = new Dictionary<Hex, List<PotentialAction>>();
        if(m_wasShutDown)
        {
            return new PotentialAction[0];
        }
        return m_systems.Where(system => system.Operational())
            .SelectMany(system => system.ActionsInRange(this, dict));
    }

    protected override double ExternalDamage(double strength, EffectType effectType)
    {
        var result = strength;
        if(Shield > 0)
        { 
            switch (effectType)
            {
                case EffectType.PhysicalDamage:
                case EffectType.EmpDamage:
                    Shield -= strength;
                    strength = -Shield;
                    break;

                case EffectType.IncendiaryDamage:
                    Shield -= strength / 2;
                    strength = - Shield;
                    break;

                case EffectType.HeatDamage:
                    //TODO - implement heat mechanics
                    Shield -= 1;
                    break;
            }
        }

        if (Shield <= 0)
        {
            result = base.ExternalDamage(strength, effectType);
            Shield = 0;
        }
        return result;
    }

    #endregion private and protected methods
}

#endregion ActiveEntity

#region MovingEntity

public abstract class MovingEntity : ActiveEntity
{
    #region private fields

    private readonly double m_maximumSpeed;

    #endregion private fields

    #region properties

    public double AvailableSteps { get; set; }

    public MovementType MovementMethod { get; private set; }

    #endregion properties

    #region constructor

    public MovingEntity(double maximumEnergy, MovementType movement, double speed, Loyalty loyalty, int radarRange, int sightRange, IEnumerable<Subsystem> systems, double health, double shield, VisualProperties visuals, EntityReactor reactor) :
        base(maximumEnergy, loyalty, radarRange, sightRange, systems, health, shield, visuals, reactor)
    {
        m_maximumSpeed = speed;
        AvailableSteps = speed;
        MovementMethod = movement;
    }

    #endregion constructor

    #region overrides

    protected override IEnumerable<PotentialAction> ComputeActions()
    {
        var baseActions = base.ComputeActions();
        var possibleHexes = AStar.FindAllAvailableHexes(Hex, AvailableSteps, MovementMethod);
        return baseActions.Union(possibleHexes.Values.Select(movement => (PotentialAction)movement));
    }

    public override bool StartTurn()
    {
        if(base.StartTurn())
        {
            AvailableSteps = m_maximumSpeed;
            return true;
        }
        else
        {
            AvailableSteps = 0;
            return false;
        }
    }

    public override string FullState()
    {
        return "{0} movement {1}".FormatWith(base.FullState(), AvailableSteps);
    }

    #endregion overrides
}

#endregion MovingEntity

#region Mech

//TODO - should be replaced with XML configuration files

public class Mech : MovingEntity
{
    public Mech(IEnumerable<Subsystem> systems, EntityReactor reactor,
                double maximumEnergy = 2,
                double health = 5,
                double shield = 3,
                VisualProperties visuals = VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight,
                double speed = 4,
                Loyalty loyalty = Loyalty.Player,
                int radarRange = 20,
                int sightRange = 10) :
        base(maximumEnergy, MovementType.Walker, speed, loyalty, radarRange, sightRange, systems, health, shield, visuals, reactor)
    { }
}

#endregion Mech