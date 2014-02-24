using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


#region Subsystem

public abstract class Subsystem
{
    #region fields

    private SystemCondition m_condition;

    private readonly int m_maxRange;
    
    private readonly int m_minRange;

    private readonly DeliveryMethod m_deliveryMethod;

    private readonly HexOperation m_effect;

    private readonly string m_buttonName;

    private readonly HexCheck m_conditionForTargeting;

    #endregion

    #region properties

    public SystemCondition OperationalCondition
    {
        get
        {
            return m_condition;
        }

        private set
        {
            //condition most be ordered by order of severity
            if (value > m_condition)
            {
                m_condition = value;
            }
        }
    }

    #endregion

    #region constructors

    protected Subsystem(int minRange, int maxRange, HexOperation effect, DeliveryMethod deliveryMethod, string buttonName, HexCheck conditionForTargeting)
    {
        m_condition = SystemCondition.Operational;
        m_minRange = minRange;
        m_maxRange = maxRange;
        m_effect = effect;
        m_deliveryMethod = deliveryMethod;
        m_buttonName = buttonName;
        m_conditionForTargeting = conditionForTargeting;
    }

    protected Subsystem(HexOperation effect, string buttonName) : 
        this(0, 0, effect, DeliveryMethod.Direct, buttonName, (hex) => true)
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
        return m_condition == SystemCondition.Operational;
    }

    public IEnumerable<PotentialAction> ActionsInRange(Hex sourceHex)
    {
        return TargetsInRange(sourceHex).Select(targetedHex => CreateAction(targetedHex));
    }

    #endregion

    #region private methods

    private PotentialAction CreateAction(Hex hex)
    {
        return new OperateSystemAction(m_effect, m_buttonName, hex);
    }

    private IEnumerable<Hex> TargetsInRange(Hex hex)
    { 
        if(m_maxRange == 0)
        {
            return new[] { hex };
        }
        
        switch (m_deliveryMethod)
        {
            case(DeliveryMethod.Direct):
                return CheckForDirectTargets(hex);

            case(DeliveryMethod.Unobstructed):
                return CheckForIndirectTargets(hex);

            default:
                throw new UnknownTypeException(m_deliveryMethod);
        }
    }

    private IEnumerable<Hex> CheckForDirectTargets(Hex hex)
    {
        if(hex.Content == null)
        {
            throw new Exception("System {0} operating out of empty hex {1}".FormatWith(this, hex));
        }

        hex.Content.Marker.collider2D.enabled = false;
        var results = new HashSet<Hex>();
        var layerMask = 1 << LayerMask.NameToLayer("Entities");
        var amountOfHexesToCheck = 6*m_maxRange - 6;

        for(int i = 0; i < amountOfHexesToCheck ; i++)
        {
            for(int j = 0 ; j < 6 ; j++)
            {
                var rayHit = Physics2D.Linecast(hex.Position, mousePosition - startingPoint, 1000, layerMask);
            }
        }

        hex.Content.Marker.collider2D.enabled = true;
        return results;
    }

    private IEnumerable<Hex> CheckForIndirectTargets(Hex startingHex)
    {
        int minRange = m_minRange;
        int maxRange = m_maxRange;
        var currentCheckedHexes = new HashSet<Hex>{startingHex};
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
    public DamageType DamageType { get; private set; }

    public bool ShieldPiercing { get; private set; }

    protected WeaponBase(int minRange, int maxRange, HexOperation effect, DeliveryMethod deliveryMethod, string buttonName, HexCheck conditionForTargeting) : 
        base(minRange, maxRange, effect, deliveryMethod, buttonName, conditionForTargeting)
    {
    }
}

public abstract class AmmoWeapon : WeaponBase
{
    private int m_ammo;

    protected AmmoWeapon(int minRange, int maxRange, HexOperation effect, DeliveryMethod deliveryMethod, string buttonName, HexCheck conditionForTargeting) : 
        base(minRange, maxRange, effect, deliveryMethod, buttonName, conditionForTargeting)
    {
    }
}

#endregion

#region weapons
//TODO - should be replaced with XML configuration files


#endregion


