using Assets.scripts.Base;
using Assets.scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    #region Subsystem

    public class Subsystem
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

        public Subsystem(Int32 id, Loyalty loyalty)
            : this(SubsystemTemplate.Init(id), loyalty)
        { }

        public Subsystem(SubsystemTemplate template, Loyalty loyalty)
        {
            m_workingCondition = SystemCondition.Operational;
            Template = template;
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
            if (actingEntity.CurrentEnergy < Template.EnergyCost)
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
            if (operation.NecessaryConditions())

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
            base(1, loyalty)
        { }
    }

    public class MissileLauncher : Subsystem
    {
        public MissileLauncher(Loyalty loyalty) :
            base(2, loyalty)
        { }
    }

    public class EmpLauncher : Subsystem
    {
        public EmpLauncher(Loyalty loyalty) :
            base(0, loyalty)
        { }
    }

    public class HeatWaveProjector : Subsystem
    {
        public HeatWaveProjector(Loyalty loyalty) :
            base(4, loyalty)
        { }
    }

    public class IncediaryGun : Subsystem
    {
        public IncediaryGun(Loyalty loyalty) :
            base(5, loyalty)
        { }
    }

    #endregion weapons
}