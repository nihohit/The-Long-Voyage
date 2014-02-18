using UnityEngine;
using System.Collections.Generic;


#region Subsystem

public abstract class Subsystem
{
    private Condition m_condition;

    public int MaxRange { get; private set; }
    public int MinRange { get; private set; }
    public abstract EntityCheck IsBlocked { get; }
    public abstract HexOperation Effect { get; }

    public Condition OperationalCondition
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

    public virtual void Hit(DamageType type)
    {
        switch (type)
        {
            case (DamageType.EMP):
                {
                    OperationalCondition = Condition.Neutralized;
                }
                break;
            default:
                OperationalCondition = Condition.Destroyed;
                break;
        }
    }

    public bool Operational()
    {
        return m_condition == Condition.Operational;
    }
}

#endregion

#region abstract weapons

public abstract class WeaponBase : Subsystem
{
    private static readonly Dictionary<WeaponType, WeaponBase> s_weaponFactory = new Dictionary<WeaponType, WeaponBase>();

    public bool DirectFire { get; private set; }
    public DamageType DamageType { get; private set; }
    public bool ShieldPiercing { get; private set; }
    public int Range { get; private set; }

    public WeaponBase GetInstance(WeaponType type)
    {
        WeaponBase result = null;
        if (!s_weaponFactory.TryGetValue(type, out result))
        {

        }
        return result;
    }
}

public abstract class AmmoWeapon : WeaponBase
{
    private int m_ammo;
}

#endregion

#region weapons
//TODO - should be replaced with XML configuration files


#endregion


