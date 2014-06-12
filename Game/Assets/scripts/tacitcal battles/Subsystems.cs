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

    private readonly double m_heatGenerated;

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

    public double HeatGenerated { get { return m_heatGenerated; } }

    public int MaxAmmo { get { return m_maxAmmo; } }

    #endregion properties

    #region constructor and initializer

    private SubsystemTemplate(double energyCost,
                              double heatGenerated,
                              int minRange,
                              int maxRange,
                              DeliveryMethod deliveryMethod,
                              string name,
                              EffectType effectType,
                              double effectStrength,
                              TargetingType targetingType)
        : this(0, energyCost, heatGenerated, minRange, maxRange, deliveryMethod, name, effectType, effectStrength, targetingType)
    { }

    private SubsystemTemplate(int ammo,
                            double energyCost,
                            double heatGenerated,
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
        m_heatGenerated = heatGenerated;
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
                                 new SubsystemTemplate(1, 0, 0, 2, DeliveryMethod.Direct, "Emp", EffectType.EmpDamage, 1.5f, TargetingType.Enemy));
            s_knownTemplates.Add(SystemType.Laser,
                                 new SubsystemTemplate(2, 2, 0, 4, DeliveryMethod.Direct, "Laser", EffectType.PhysicalDamage, 2f, TargetingType.Enemy));
            s_knownTemplates.Add(SystemType.Missile,
                                 new SubsystemTemplate(4, 1, 0, 2, 6, DeliveryMethod.Unobstructed, "Missile", EffectType.PhysicalDamage, 1f, TargetingType.Enemy));
            s_knownTemplates.Add(SystemType.Flamer,
                                 new SubsystemTemplate(2, 2, 2, 0, 2, DeliveryMethod.Unobstructed, "Flamer", EffectType.FlameHex, 1f, TargetingType.AllHexes));
            s_knownTemplates.Add(SystemType.HeatWave,
                                 new SubsystemTemplate(2, 1, 0, 3, DeliveryMethod.Direct, "HeatWave", EffectType.HeatDamage, 2f, TargetingType.Enemy));
            s_knownTemplates.Add(SystemType.IncediaryGun,
                                 new SubsystemTemplate(10, 0.5, 0.5, 0, 4, DeliveryMethod.Direct, "IncediaryGun", EffectType.IncendiaryDamage, 1.5f, TargetingType.Enemy));
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

    public SubsystemTemplate Template { get; private set; }

    #endregion properties

    #region constructors

    protected Subsystem(SystemType type, Loyalty loyalty)
    {
        m_workingCondition = SystemCondition.Operational;
        Template = SubsystemTemplate.Init(type);
        m_conditionForTargeting = CreateTargetingCheck(loyalty, Template.PossibleTargets);
        var effect = CreateSystemEffect(Template.EffectStrength, Template.Effect);
        if (Template.MaxAmmo > 0)
        {
            m_ammo = Template.MaxAmmo;
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
        //TODO - decide on a relevant value
        if (Randomiser.ProbabilityCheck(damage / 5))
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
        }
        Debug.Log("{0} was hit for {1} {2} damage, it is now {3}".FormatWith(Template.Name, damage, type, OperationalCondition));
    }

    public bool Operational()
    {
        return m_workingCondition == SystemCondition.Operational;
    }

    public IEnumerable<OperateSystemAction> ActionsInRange(ActiveEntity actingEntity, Dictionary<Hex, List<OperateSystemAction>> dict)
    {
        //if we can't operate the system, return no actions
        if(actingEntity.CurrentEnergy < Template.EnergyCost)
        {
            return new OperateSystemAction[0];
        }
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

    private OperateSystemAction CreateAction(ActiveEntity actingEntity, Hex hex, Dictionary<Hex, List<OperateSystemAction>> dict)
    {
        var list = dict.TryGetOrAdd(hex, () => new List<OperateSystemAction>());
        Assert.EqualOrLesser(list.Count, 6, "Too many subsystems");

        var operation = new OperateSystemAction(actingEntity, m_effect, Template, hex);
        if(operation.NecessaryConditions())

        list.Add(operation);
        return operation;
    }

    private IEnumerable<Hex> TargetsInRange(Hex hex)
    {
        if (Template.MaxRange == 0)
        {
            return new[] { hex };
        }

        var layerName = "Entities";

        switch (Template.DeliveryMethod)
        {
            case (DeliveryMethod.Direct):
                return hex.RaycastAndResolve(Template.MinRange, Template.MaxRange, m_conditionForTargeting, false, layerName);

            case (DeliveryMethod.Unobstructed):
                return hex.RaycastAndResolve(Template.MinRange, Template.MaxRange, m_conditionForTargeting, true, layerName);

            default:
                throw new UnknownTypeException(Template.DeliveryMethod);
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

public class HeatWaveProjector : Subsystem
{
    public HeatWaveProjector(Loyalty loyalty) :
        base(SystemType.HeatWave, loyalty)
    { }
}

public class IncediaryGun : Subsystem
{
    public IncediaryGun(Loyalty loyalty) :
        base(SystemType.IncediaryGun, loyalty)
    { }
}

#endregion weapons