using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region SubsystemTemplate

public class SubsystemTemplate
{
    #region fields

    private static readonly Dictionary<SystemType, SubsystemTemplate> s_knownTemplates = new Dictionary<SystemType, SubsystemTemplate>();

    private readonly int m_maxRange;

    private readonly int m_minRange;

    private readonly DeliveryMethod m_deliveryMethod;

    private readonly EffectType m_effectType;

    private readonly string m_name;

    private readonly TargetingType m_targetingType;

    private readonly double m_energyCost;

    private readonly double m_effectStrength;

    private readonly int m_maxAmmo;

    #endregion fields

    #region properties

    public int MaxRange { get { return m_maxRange; } }

    public int MinRange { get { return m_minRange; } }

    public DeliveryMethod DeliveryMethod { get { return m_deliveryMethod; } }

    public EffectType Effect { get { return m_effectType; } }

    public double EffectStrength { get { return m_effectStrength; } }

    public string Name { get { return m_name; } }

    public TargetingType PossibleTargets { get { return m_targetingType; } }

    public double EnergyCost { get { return m_energyCost; } }

    public int MaxAmmo { get { return m_maxAmmo; } }

    #endregion properties

    #region constructor and initializer

    private SubsystemTemplate(double energyCost,
                              int minRange,
                              int maxRange,
                              DeliveryMethod deliveryMethod,
                              string name,
                              EffectType effectType,
                              double effectStrength,
                              TargetingType targetingType)
        : this(0, energyCost, minRange, maxRange, deliveryMethod, name, effectType, effectStrength, targetingType)
    { }

    private SubsystemTemplate(int ammo,
                              double energyCost,
                              int minRange,
                              int maxRange,
                              DeliveryMethod deliveryMethod,
                              string name,
                              EffectType effectType,
                              double effectStrength,
                              TargetingType targetingType)
    {
        m_minRange = minRange;
        m_maxRange = maxRange;
        m_effectType = effectType;
        m_effectStrength = effectStrength;
        m_deliveryMethod = deliveryMethod;
        m_name = name;
        m_targetingType = targetingType;
        m_energyCost = energyCost;
        m_maxAmmo = ammo;
    }

    public static SubsystemTemplate Init(SystemType type)
    {
        //TODO - no error handling at the moment.
        return s_knownTemplates[type];
    }

    //TODO - this method should be removed after we have initialization from XML
    public static void Init()
    {
        if (s_knownTemplates.Count == 0)
        {
            s_knownTemplates.Add(SystemType.EMP,
                                 new SubsystemTemplate(1, 0, 2, DeliveryMethod.Direct, "Emp", EffectType.EmpDamage, 1.5f, TargetingType.Enemy));
            s_knownTemplates.Add(SystemType.Laser,
                                 new SubsystemTemplate(2, 0, 4, DeliveryMethod.Direct, "Laser", EffectType.PhysicalDamage, 2f, TargetingType.Enemy));
            s_knownTemplates.Add(SystemType.Missile,
                                 new SubsystemTemplate(4, 1, 2, 6, DeliveryMethod.Unobstructed, "Missile", EffectType.PhysicalDamage, 1f, TargetingType.Enemy));
        }
    }

    #endregion constructor and initializer
}

#endregion SubsystemTemplate

#region Subsystem

public abstract class Subsystem
{
    #region fields

    private SystemCondition m_workingCondition;

    private readonly SubsystemTemplate m_template;

    private int m_ammo;

    private readonly HexCheck m_conditionForTargeting;

    private readonly HexOperation m_effect;

    #endregion fields

    #region properties

    public SystemCondition OperationalCondition
    {
        get
        {
            return m_workingCondition;
        }

        private set
        {
            //condition most be ordered by order of severity
            if (value > m_workingCondition)
            {
                m_workingCondition = value;
            }
        }
    }

    #endregion properties

    #region constructors

    protected Subsystem(SystemType type, Loyalty loyalty)
    {
        m_workingCondition = SystemCondition.Operational;
        m_template = SubsystemTemplate.Init(type);
        m_conditionForTargeting = CreateTargetingCheck(loyalty, m_template.PossibleTargets);
        var effect = CreateSystemEffect(m_template.EffectStrength, m_template.Effect);
        if (m_template.MaxAmmo > 0)
        {
            m_ammo = m_template.MaxAmmo;
            m_effect = (hex) =>
            {
                effect(hex);
                --m_ammo;
                if (m_ammo == 0)
                {
                    m_workingCondition = SystemCondition.OutOfAmmo;
                }
            };
        }
        else
        {
            m_effect = effect;
        }
    }

    #endregion constructors

    #region public methods

    public void Hit(EffectType type, double damage)
    {
        switch (type)
        {
            case (EffectType.EmpDamage):
                OperationalCondition = SystemCondition.Neutralized;
                break;

            case (EffectType.PhysicalDamage):
                OperationalCondition = SystemCondition.Destroyed;
                break;
        }
        Debug.Log("{0} was hit for {1} {2} damage, it is now {3}".FormatWith(m_template.Name, damage, type, OperationalCondition));
    }

    public bool Operational()
    {
        return m_workingCondition == SystemCondition.Operational;
    }

    public IEnumerable<PotentialAction> ActionsInRange(ActiveEntity actingEntity, Dictionary<Hex, List<PotentialAction>> dict)
    {
        //TODO - noe longer targeting inactive objects. This should be removed.
        return TargetsInRange(actingEntity.Hex).Select(targetedHex => CreateAction(actingEntity, targetedHex, dict));
    }

    #endregion public methods

    #region private methods

    private static HexOperation CreateSystemEffect(double effectStrength, EffectType damageType)
    {
        return (hex) =>
        {
            hex.Content.Affect(effectStrength, damageType);
        };
    }

    private static HexCheck CreateTargetingCheck(Loyalty loyalty, TargetingType targeting)
    {
        return (hex) =>
        {
            return ((targeting & TargetingType.AllHexes) != 0) ||
                (((targeting & TargetingType.Enemy) != 0) && (hex.Content != null && hex.Content.Loyalty != loyalty)) ||
                (((targeting & TargetingType.Friendly) != 0) && (hex.Content != null && hex.Content.Loyalty == loyalty));
        };
    }

    private PotentialAction CreateAction(ActiveEntity actingEntity, Hex hex, Dictionary<Hex, List<PotentialAction>> dict)
    {
        Vector2 offset = Vector2.zero;
        var list = dict.TryGetOrAdd(hex, () => new List<PotentialAction>());
        var size = ((CircleCollider2D)hex.Reactor.collider2D).radius;
        Assert.EqualOrLesser(list.Count, 6, "Too many subsystems");

        switch (list.Count(action => !action.Destroyed))
        {
            case (0):
                offset = new Vector2(-(size), 0);
                break;

            case (1):
                offset = new Vector2(-(size / 2), (size));
                break;

            case (2):
                offset = new Vector2((size / 2), (size));
                break;

            case (3):
                offset = new Vector2(size, 0);
                break;

            case (4):
                offset = new Vector2(size / 2, -size);
                break;

            case (5):
                offset = new Vector2(-(size / 2), -size);
                break;
        }

        var operation = new OperateSystemAction(actingEntity, m_effect, m_template.Name, hex, offset, m_template.EnergyCost);
        list.Add(operation);
        return operation;
    }

    private IEnumerable<Hex> TargetsInRange(Hex hex)
    {
        if (m_template.MaxRange == 0)
        {
            return new[] { hex };
        }

        var layerName = "Entities";

        switch (m_template.DeliveryMethod)
        {
            case (DeliveryMethod.Direct):
                return hex.RaycastAndResolve(m_template.MinRange, m_template.MaxRange, m_conditionForTargeting, false, layerName);

            case (DeliveryMethod.Unobstructed):
                return hex.RaycastAndResolve(m_template.MinRange, m_template.MaxRange, m_conditionForTargeting, true, layerName);

            default:
                throw new UnknownTypeException(m_template.DeliveryMethod);
        }
    }

    #endregion private methods
}

#endregion Subsystem

#region weapons

//TODO - should be replaced with XML configuration files

public class Laser : Subsystem
{
    public Laser(Loyalty loyalty) :
        base(SystemType.Laser, loyalty)
    { }
}

public class MissileLauncher : Subsystem
{
    public MissileLauncher(Loyalty loyalty) :
        base(SystemType.Missile, loyalty)
    { }
}

public class EmpLauncher : Subsystem
{
    public EmpLauncher(Loyalty loyalty) :
        base(SystemType.EMP, loyalty)
    { }
}

#endregion weapons