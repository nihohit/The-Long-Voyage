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

    #endregion

    #region constructor

    public Entity(Loyalty loyalty, double health, double shield, VisualProperties visuals, EntityReactor reactor)
    {
        Loyalty = loyalty;
        Health = health;
        Shield = shield;
        Visuals = visuals;
        reactor.Entity = this;
        this.Marker = reactor;
        m_id = s_idCounter++;
        m_name = "{0} {1} {2}".FormatWith(this.GetType().ToString(), Loyalty, m_id);
    }

    #endregion

    #region properties

    public int ID { get {return m_id;}}

    public MarkerScript Marker { get; set; }

    public double Health { get; private set; }

    public double Shield { get; private set; }

    public VisualProperties Visuals { get; private set; }

    public virtual Hex Hex { get; set; }

    public Loyalty Loyalty { get; private set; }

    public String Name { get { return m_name; } }

    #endregion

    #region public methods

    public virtual void Hit(double damage, DamageType damageType)
    {
        Debug.Log("{0} was hit for damage {1} and type {2}".FormatWith(m_name, damage, damageType));
        //TODO - handle damage types
        Health -= damage;
        Debug.Log("{0} has now {1} health and {2} shields".FormatWith(m_name, Health, Shield));
        if(Destroyed())
        {
            Destroy();
        }
    }

    // this function returns a string value that represents the mutable state of the entity
    public virtual string FullState()
    {
        return "Health {0} shields {1} Hex {2}".FormatWith(Health, Shield, Hex);
    }

    // just a simple function to make the code more readable
    public bool Destroyed()
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

    #endregion

    #endregion

    #region private methods

    private void Destroy()
    {
        Debug.Log("Destroy {0}".FormatWith(m_name));
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

    public TerrainEntity(double health, bool visibleOnRadar, bool blocksSight, EntityReactor reactor)
        : base(Loyalty.Neutral, health, 0, 
               VisualProperties.AppearsOnSight | (visibleOnRadar ? VisualProperties.AppearsOnRadar : VisualProperties.None) | (blocksSight ? VisualProperties.BlocksSight : VisualProperties.None), reactor)
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
    public DenseTrees(EntityReactor reactor) : base(FileHandler.GetIntProperty("Dense trees health", FileAccessor.Units), false, true, reactor)
    { }
}

public class SparseTrees : TerrainEntity
{
    public SparseTrees(EntityReactor reactor) : base(FileHandler.GetIntProperty("Sparse trees health", FileAccessor.Units), false, false, reactor)
    { }
}

public class Building : TerrainEntity
{
    public Building(EntityReactor reactor) : base(FileHandler.GetIntProperty("Building health", FileAccessor.Units), true, true, reactor)
    { }
}

#endregion

#region ActiveEntity

public abstract class ActiveEntity : Entity
{
    #region constructor

    public ActiveEntity(double maximumEnergy, Loyalty loyalty, int radarRange, int sightRange, IEnumerable<Subsystem> systems, double health, double shield, VisualProperties visuals, EntityReactor reactor) :
        base(loyalty, health, shield, visuals, reactor)
    {
        m_radarRange = radarRange;
        m_sightRange = sightRange;
        m_systems = systems;
        m_maximumEnergy = maximumEnergy;
        CurrentEnergy = maximumEnergy;
    }

    #endregion

    #region private fields
    
    private HashSet<Hex> m_detectedHexes;

    private readonly int m_radarRange;

    private readonly int m_sightRange;

    private readonly IEnumerable<Subsystem> m_systems;

    private IEnumerable<PotentialAction> m_actions;

    private readonly double m_maximumEnergy;

    #endregion

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

    #endregion

    #region public methods

    public void ResetActions()
    {
        Debug.Log("{0} is resetting actions".FormatWith(Name));
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
        Debug.Log("{0} is setting seen hexes".FormatWith(Name));

        if(SeenHexes != null)
        {
            var whatTheEntitySeesNowSet = new HashSet<Hex>(whatTheEntitySeesNow);
            var whatTheEntitySeesNowInRadarSet = new HashSet<Hex>(whatTheEntitySeesNowInRadar);
            //TODO - we can remove this optimization and just call ResetSeenHexes before this, but it'll be more expensive. Until it'll cause problem I'm keeping this
            //this leaves in each list the hexes not in the other
            whatTheEntitySeesNowSet.ExceptOnBoth(SeenHexes);
            whatTheEntitySeesNowInRadarSet.ExceptOnBoth(m_detectedHexes);

            foreach(var hex in SeenHexes)
            {
                hex.Unseen();
            }
            foreach(var hex in m_detectedHexes)
            {
                hex.Undetected();
            }
            foreach(var hex in whatTheEntitySeesNowSet)
            {
                hex.Seen();
            }
            foreach(var hex in whatTheEntitySeesNowInRadarSet)
            {
                hex.Detected();
            }
        }
        else
        {
            foreach(var hex in whatTheEntitySeesNow)
            {
                hex.Seen();
            }
            foreach(var hex in whatTheEntitySeesNowInRadar)
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

    public virtual void StartTurn()
    {
        ResetActions();
        ResetSeenHexes();
        CurrentEnergy = m_maximumEnergy;
    }

    public override string FullState()
    {
        return "{0} Energy {1}".FormatWith(base.FullState(), CurrentEnergy);
    }

    #endregion

    #region private and protected methods

    private IEnumerable<Hex> FindSeenHexes()
    {
        //TODO - we might be able to make this somewhat more efficient by combining the sight & radar raycasts, but we should first make sure that it is needed.
        return Hex.RaycastAndResolve<HexReactor>(0, m_sightRange, (hex) => true, true, (hex) => (hex.Content != null  && ((hex.Content.Visuals & VisualProperties.BlocksSight) != 0)) ,"Hexes", (reactor)=> reactor.MarkedHex);
    }
    
    private IEnumerable<Hex> FindRadarHexes()
    {
        //TODO - we might be able to make this somewhat more efficient by combining the sight & radar raycasts, but we should first make sure that it is needed.
        return Hex.RaycastAndResolve(0, m_radarRange, (hex) => hex.Content != null, true, "Entities");
    }

    protected virtual IEnumerable<PotentialAction> ComputeActions()
    {
        Debug.Log("{0} is computing actions. Its condition is {1}".FormatWith(this, FullState()));
        var dict = new Dictionary<Hex, List<PotentialAction>>();
        return m_systems.Where(system => system.Operational())
            .SelectMany(system => system.ActionsInRange(this, dict));
    }

    #endregion
}

#endregion

#region MovingEntity

public abstract class MovingEntity : ActiveEntity
{
    #region private fields

    private readonly double m_maximumSpeed;

    #endregion

    #region properties

    public double AvailableSteps { get ; set; }
    public MovementType MovementMethod { get; private set; }

    #endregion

    #region constructor

    public MovingEntity(double maximumEnergy, MovementType movement, double speed, Loyalty loyalty, int radarRange, int sightRange, IEnumerable<Subsystem> systems, double health, double shield, VisualProperties visuals, EntityReactor reactor) :
        base(maximumEnergy, loyalty, radarRange, sightRange, systems, health, shield, visuals, reactor)
    {
        m_maximumSpeed = speed;
        AvailableSteps = speed;
        MovementMethod = movement;
    }

    #endregion

    #region overrides

    protected override IEnumerable<PotentialAction> ComputeActions()
    {
        var baseActions = base.ComputeActions();
        var possibleHexes = AStar.FindAllAvailableHexes(Hex, AvailableSteps, MovementMethod);
        return baseActions.Union(possibleHexes.Values.Select(movement => (PotentialAction)movement));
    }

    public override void StartTurn()
    {
        base.StartTurn();
        AvailableSteps = m_maximumSpeed;
    }

    public override string FullState()
    {
        return "{0} movement {1}".FormatWith(base.FullState(), AvailableSteps);
    }

    #endregion
}

#endregion

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

#endregion