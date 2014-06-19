using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EntityType { Mech, SparseTrees, DenseTrees, Building }

#region EntityTemplate

//TODO - we can create different levels of templates for the different entities. Not sure it's needed now
public class EntityTemplate
{
    #region fields

    private static readonly Dictionary<EntityType, EntityTemplate> s_knownTemplates = new Dictionary<EntityType, EntityTemplate>();

    #endregion

    #region properties

    //TODO - if we'll want entities with fixed systems, we'll need to add their templates here and merge them in the entity constructor
    public double Health { get; private set; }

    public VisualProperties Visuals { get; private set; }

    public String Name { get; private set; }

    public double Armor { get; private set; }

    public int RadarRange { get; private set; }

    public int SightRange { get; private set; }

    public double MaxEnergy { get; private set; }

    public double MaxHeat { get; private set; }

    public double MaxShields { get; private set; }

    public double HeatLossRate { get; private set; }

    public double ShieldRechargeRate { get; private set; }

    public MovementType MovementMethod { get; private set; }

    public double MaxSpeed { get; private set; }

    #endregion

    #region constructors

    //for inanimate entities
    private EntityTemplate(string name, int health, VisualProperties visualProperties) :
        this(name, health, visualProperties, 0, 0, 0, 0, 0, 0, 0, 0)
    { }

    //for unmoving entities
    private EntityTemplate(string name, int health, VisualProperties visualProperties, double armor,
        int radarRange, int sightRange, double maxEnergy, double maxHeat, double maxShields,
        double heatLossRate, double shieldRechargeRate) :
        this(name, health, visualProperties, armor, radarRange, sightRange, maxEnergy, maxHeat, maxShields, heatLossRate, shieldRechargeRate, MovementType.Unmoving, 0)
    { }

    private EntityTemplate(string name, int health, VisualProperties visualProperties, double armor,
        int radarRange, int sightRange, double maxEnergy, double maxHeat, double maxShields,
        double heatLossRate, double shieldRechargeRate, MovementType movementType, double maximumSpeed)
    {
        Health = health;
        Visuals = visualProperties;
        Armor = armor;
        RadarRange = radarRange;
        SightRange = sightRange;
        MaxEnergy = maxEnergy;
        MaxHeat = maxHeat;
        MaxShields = maxShields;
        HeatLossRate = heatLossRate;
        ShieldRechargeRate = shieldRechargeRate;
        MovementMethod = movementType;
        MaxSpeed = maximumSpeed;
    }

    #endregion

    #region static methods

    public static EntityTemplate GetTemplate(EntityType type)
    {
        //TODO - no error handling at the moment.
        return s_knownTemplates[type];
    }

    //TODO - this method should be removed after we have initialization from XML
    public static void Init()
    {
        if (s_knownTemplates.Count == 0)
        {
            s_knownTemplates.Add(EntityType.Mech, new EntityTemplate("Mech", 5,
                VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight,
                0, 20, 10, 2, 5, 3, 1, 1, MovementType.Walker, 4));
            s_knownTemplates.Add(EntityType.DenseTrees, new EntityTemplate("Dense trees",
                FileHandler.GetIntProperty("Dense trees health", FileAccessor.Units), VisualProperties.AppearsOnSight | VisualProperties.BlocksSight));
            s_knownTemplates.Add(EntityType.SparseTrees, new EntityTemplate("Sparse trees",
                FileHandler.GetIntProperty("Sparse trees health", FileAccessor.Units), VisualProperties.AppearsOnSight));
            s_knownTemplates.Add(EntityType.Building, new EntityTemplate("Building",
                FileHandler.GetIntProperty("Building health", FileAccessor.Units), VisualProperties.AppearsOnSight | VisualProperties.BlocksSight | VisualProperties.AppearsOnRadar));
        }
    }

    #endregion
}

#endregion

#region Entity

public abstract class Entity
{
    #region private fields

    private static int s_idCounter = 0;

    private readonly int m_id;

    #endregion private fields

    #region constructor

    public Entity(EntityTemplate template, Loyalty loyalty, EntityReactor reactor)
    {
        Template = template;
        Loyalty = loyalty;
        Health = template.Health;
        reactor.Entity = this;
        this.Reactor = reactor;
        m_id = s_idCounter++;
        Name = "{0} {1} {2}".FormatWith(template.Name, Loyalty, m_id);
        if((template.Visuals & VisualProperties.AppearsOnRadar) != 0)
        {
            TacticalState.AddRadarVisibleEntity(this);
        }
    }

    #endregion constructor

    #region properties

    public int ID { get { return m_id; } }

    public EntityReactor Reactor { get; private set; }

    public double Health { get; private set; }

    public virtual Hex Hex { get; set; }

    public Loyalty Loyalty { get; private set; }

    public String Name { get; private set; }

    public EntityTemplate Template { get; private set; }

    #endregion properties

    #region public methods

    public void Affect(double strength, EffectType effectType)
    {
        Debug.Log("{0} was hit for damage {1} and type {2}".FormatWith(Name, strength, effectType));
        var remainingDamage = ExternalDamage(strength, effectType);
        InternalDamage(remainingDamage, effectType);
        Debug.Log(FullState());

        if (Destroyed())
        {
            Destroy();
        }
    }

    // this function returns a string value that represents the mutable state of the entity
    public virtual string FullState()
    {
        return "{0}: Health {1}/{2} Hex {3}".FormatWith(Name, Health, Template.Health, Hex);
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
        return Hasher.GetHashCode(Name, Reactor, m_id);
    }

    public override string ToString()
    {
        return Name;
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
                //TODO - Can armor be ablated away? if so, it needs to be copied over into a local field in the entity
                return strength - Template.Armor;
                
            case EffectType.EmpDamage:
            case EffectType.HeatDamage:
                return strength;


            default:
                throw new UnknownTypeException(damageType);
        }
    }

    protected virtual void Destroy()
    {
        Debug.Log("Destroy {0}".FormatWith(Name));
        this.Hex.Content = null;
        TacticalState.DestroyEntity(this);
        UnityEngine.Object.Destroy(this.Reactor.gameObject);
    }

    #endregion protected methods
}

#endregion Entity

#region inanimate entities

public class TerrainEntity : Entity
{
    private Hex m_hex;

    public TerrainEntity(EntityTemplate template, EntityReactor reactor)
        : base(template, Loyalty.Inactive, reactor)
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

#endregion inanimate entities

#region ActiveEntity

public class ActiveEntity : Entity
{
    #region constructor

    public ActiveEntity(EntityTemplate template, Loyalty loyalty, EntityReactor reactor, IEnumerable<Subsystem> systems) :
        base(template, loyalty, reactor)
    {
        m_systems = systems;
        CurrentEnergy = template.MaxEnergy;
        m_tempMaxEnergy = template.MaxEnergy;
        Shield = template.MaxShields;
    }

    #endregion constructor

    #region private fields

    private HashSet<Hex> m_detectedHexes;

    private readonly IEnumerable<Subsystem> m_systems;

    private IEnumerable<PotentialAction> m_actions;

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

    public double CurrentHeat { get; set; }

    public IEnumerable<Subsystem> Systems { get { return m_systems.Where(system => system.Operational()); } }

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
        CurrentHeat = Math.Max(CurrentHeat - Template.HeatLossRate, 0);
        if (m_tempMaxEnergy <= 0 || CurrentHeat >= Template.MaxHeat)
        {
            if (CurrentHeat >= Template.MaxHeat) CurrentHeat = 0;
            m_tempMaxEnergy = Template.MaxEnergy;
            m_wasShutDown = true;
            return false;
        }
        m_wasShutDown = false;
        m_tempMaxEnergy = Template.MaxEnergy;
        Shield = Math.Min(Template.MaxShields, Shield + Template.ShieldRechargeRate);
        return true;
    }

    public override string FullState()
    {
        return "{0} Shields {5}/{6} Heat {3}/{4} Energy {1}/{2} ".
            FormatWith(base.FullState(), CurrentEnergy, Template.MaxEnergy, CurrentHeat, Template.MaxHeat, Shield, Template.MaxShields);
    }

    public override bool Destroyed()
    {
        return base.Destroyed() || m_systems.None(system => system.Operational()) || ((m_tempMaxEnergy <= 0 || CurrentHeat >= Template.MaxHeat) && m_wasShutDown);
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
        return Hex.RaycastAndResolve<HexReactor>(0, Template.SightRange, (hex) => true, true, (hex) => (hex.Content != null && ((hex.Content.Template.Visuals & VisualProperties.BlocksSight) != 0)), "Hexes", (reactor) => reactor.MarkedHex);
    }

    private IEnumerable<Hex> FindRadarHexes()
    {
        var inactiveRadarVisibleEntityMarkers = TacticalState.RadarVisibleEntities.Where(ent => !ent.Reactor.enabled).Select(ent => ent.Reactor);
        inactiveRadarVisibleEntityMarkers.ForEach(marker => marker.GetComponent<Collider2D>().enabled = true);
        var results = Hex.RaycastAndResolve(0, Template.RadarRange, (hex) => hex.Content != null, true, "Entities");
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

        CurrentHeat += heatDamage;
        m_tempMaxEnergy -= energyDamage;

        if (m_systems.Any(system => system.Operational()))
        {
            m_systems.Where(system => system.Operational()).ChooseRandomValue().Hit(damageType, physicalDamage + energyDamage);
        }

        base.InternalDamage(physicalDamage, EffectType.PhysicalDamage);
    }

    protected virtual IEnumerable<PotentialAction> ComputeActions()
    {
        Debug.Log("{0} is computing actions".FormatWith(FullState()));
        if(m_wasShutDown)
        {
            return new PotentialAction[0];
        }

        var dict = new Dictionary<Hex, List<OperateSystemAction>>();
        var results = m_systems.Where(system => system.Operational())
            .SelectMany(system => system.ActionsInRange(this, dict)).Materialize();

        foreach(var hex in dict.Keys)
        {
            hex.Reactor.AddCommands(this, dict[hex]);
        }

        return results.Select(subsytemAction => (PotentialAction)subsytemAction);
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

public class MovingEntity : ActiveEntity
{

    #region properties

    public double AvailableSteps { get; set; }

    #endregion properties

    #region constructor

    public MovingEntity(EntityTemplate template, Loyalty loyalty, EntityReactor reactor, IEnumerable<Subsystem> systems) :
        base(template, loyalty, reactor, systems)
    {
        AvailableSteps = template.MaxSpeed;
    }

    #endregion constructor

    #region overrides

    protected override IEnumerable<PotentialAction> ComputeActions()
    {
        var baseActions = base.ComputeActions();
        var possibleHexes = AStar.FindAllAvailableHexes(Hex, AvailableSteps, Template.MovementMethod);
        return baseActions.Union(possibleHexes.Values.Select(movement => (PotentialAction)movement));
    }

    public override bool StartTurn()
    {
        if(base.StartTurn())
        {
            AvailableSteps = Template.MaxSpeed;
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
        return "{0} movement {1}/{2}".FormatWith(base.FullState(), AvailableSteps, Template.MaxSpeed);
    }

    #endregion overrides
}

#endregion MovingEntity