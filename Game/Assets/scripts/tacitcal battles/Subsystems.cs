using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

#region Subsystem

public abstract class Subsystem
{
    #region fields

    private SystemCondition m_workingCondition;

    private readonly int m_maxRange;
    
    private readonly int m_minRange;

    private readonly DeliveryMethod m_deliveryMethod;

    private readonly HexOperation m_effect;

    private readonly string m_buttonName;

    private readonly HexCheck m_conditionForTargeting;

    private readonly double m_energyCost;

    #endregion

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

    #endregion

    #region constructors

    protected Subsystem(double energyCost, int minRange, int maxRange, HexOperation effect, DeliveryMethod deliveryMethod, string buttonName, HexCheck conditionForTargeting)
    {
        m_workingCondition = SystemCondition.Operational;
        m_minRange = minRange;
        m_maxRange = maxRange;
        m_effect = effect;
        m_deliveryMethod = deliveryMethod;
        m_buttonName = buttonName;
        m_conditionForTargeting = conditionForTargeting;
        m_energyCost = energyCost;
    }

    //for self-targeting systems
    protected Subsystem(double energyCost, HexOperation effect, string buttonName) : 
        this(energyCost, 0, 0, effect, DeliveryMethod.Direct, buttonName, (hex) => true)
    { }

    #endregion

    #region public methods

    public virtual void Hit(DamageType type)
    {
        switch (type)
        {
            case (DamageType.EMP):
                {
                    OperationalCondition = SystemCondition.Neutralized;
                }
                break;
            default:
                OperationalCondition = SystemCondition.Destroyed;
                break;
        }
    }

    public bool Operational()
    {
        return m_workingCondition == SystemCondition.Operational;
    }

    public IEnumerable<PotentialAction> ActionsInRange(ActiveEntity actingEntity, Dictionary<Hex, List<PotentialAction>> dict)
    {
        return TargetsInRange(actingEntity.Hex).Select(targetedHex => CreateAction(actingEntity, targetedHex, dict));
    }

    #endregion

    #region private methods

    private PotentialAction CreateAction(ActiveEntity actingEntity, Hex hex, Dictionary<Hex, List<PotentialAction>> dict)
    {
        Vector2 offset = Vector2.zero;
        var list = dict.TryGetOrAdd(hex, () => new List<PotentialAction>());
        var size = ((CircleCollider2D)hex.Reactor.collider2D).radius;
        Assert.EqualOrLesser(list.Count, 6, "Too many subsystems");

        switch(list.Count(action => !action.Destroyed))
        {
            case(0):
                offset = new Vector2(-(size), 0);
                break;
            case(1):
                offset = new Vector2(-(size/2), (size));
                break;
            case(2):
                offset = new Vector2((size/2), (size));
                break;
            case(3):
                offset = new Vector2(size, 0);
                break;
            case(4):
                offset = new Vector2(size/2, -size);
                break;
            case(5):
                offset = new Vector2(-(size/2), -size);
                break;
        }

        var operation = new OperateSystemAction(actingEntity, m_effect, m_buttonName, hex, offset, m_energyCost);
        list.Add(operation);
        return operation;
    }

    private IEnumerable<Hex> TargetsInRange(Hex hex)
    { 
        if(m_maxRange == 0)
        {
            return new[] { hex };
        }

        var layerName = "Entities";
        
        switch (m_deliveryMethod)
        {
            case(DeliveryMethod.Direct):
                return hex.RaycastAndResolve(m_minRange, m_maxRange, m_conditionForTargeting, false, layerName);

            case(DeliveryMethod.Unobstructed):
                return hex.RaycastAndResolve(m_minRange, m_maxRange, m_conditionForTargeting, true, layerName);

            default:
                throw new UnknownTypeException(m_deliveryMethod);
        }
    }

    private IEnumerable<Hex> CheckForIndirectTargets(Hex sourceHex)
    {
        int minRange = m_minRange;
        int maxRange = m_maxRange;
        var currentCheckedHexes = new HashSet<Hex>{sourceHex};
        var checkedHexes = new HashSet<Hex>();
        var result = new HashSet<Hex>();

        while(maxRange > 0)
        {
            if(minRange <= 0)
            {
                result.UnionWith(currentCheckedHexes.Where(currentHex => m_conditionForTargeting(currentHex)));
            }

            checkedHexes.UnionWith(currentCheckedHexes);
            //the next hexes we'll check are all the neighbours which we still didn't check
            currentCheckedHexes = new HashSet<Hex>(
                currentCheckedHexes.SelectMany(currentHex => currentHex.GetNeighbours().
                    Where(hexToAdd => !checkedHexes.Contains(hexToAdd))));
            minRange--;
            maxRange--;
        }

        return result;
    }

    #endregion
}

#endregion

#region abstract weapons

public abstract class WeaponBase : Subsystem
{
    protected WeaponBase(double energyCost, int minRange, int maxRange, DeliveryMethod deliveryMethod, string buttonName, double damage, DamageType damageType, Loyalty loyalty) : 
        base(energyCost, minRange, maxRange, CreateWeaponHit(damage, damageType), deliveryMethod, buttonName, CreateTargetingCheck(loyalty))
    {
    }

    private static HexOperation CreateWeaponHit(double damage, DamageType damageType)
    {
        return (hex) => 
        {
            Assert.NotNull(hex.Content, "hex.Content");
            hex.Content.Hit(damage, damageType);
        };
    }

    private static HexCheck CreateTargetingCheck(Loyalty loyalty)
    {
        return (hex) => 
        {
            return hex.Content != null &&
                hex.Content.Loyalty != loyalty;
        };
    }
}

public abstract class AmmoWeapon : WeaponBase
{
    private int m_ammo;

    protected AmmoWeapon(double energyCost, int minRange, int maxRange, DeliveryMethod deliveryMethod, string buttonName, double damage, DamageType damageType, Loyalty loyalty) : 
        base(energyCost, minRange, maxRange, deliveryMethod, buttonName, damage, damageType, loyalty)
    {
    }
}

#endregion

#region weapons
//TODO - should be replaced with XML configuration files

public class Laser : WeaponBase
{
    public Laser(Loyalty loyalty) : 
        base(2, 0,4, DeliveryMethod.Direct, "LaserCommand", 1f, DamageType.Energy, loyalty)
    {}
}

public class MissileLauncher : WeaponBase
{
    public MissileLauncher(Loyalty loyalty) : 
        base(1, 2,6, DeliveryMethod.Unobstructed, "MissileCommand", 1f, DamageType.Physical, loyalty)
    {}
}

#endregion
