using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    #region Subsystem

    public class Subsystem
    {
        #region fields

        private SystemCondition m_workingCondition;

        private int m_ammo;

        private readonly ActiveEntity m_containingEntity;

        private readonly HexEffectTemplate m_hexEffectTemplate;

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

        public Subsystem(SubsystemTemplate template, ActiveEntity containingEntity)
        {
            m_containingEntity = containingEntity;
            m_workingCondition = SystemCondition.Operational;
            Template = template;
            m_hexEffectTemplate = Template.HexEffect;
        }

        #endregion constructors

        #region public methods

        public void Effect(HexReactor targetHex)
        {
            if (Template.Effect != EntityEffectType.None && targetHex.Content != null)
            {
                targetHex.Content.Affect(Template.EffectStrength, Template.Effect);
            }
            if (m_hexEffectTemplate != null)
            {
                HexEffect.Create(m_hexEffectTemplate, targetHex);
            }
            if (m_ammo > 0)
            {
                --m_ammo;
                if (m_ammo == 0)
                {
                    m_workingCondition = SystemCondition.OutOfAmmo;
                }
            }
        }

        public void Hit(EntityEffectType type, double damage)
        {
            //TODO - decide on a relevant value.
            // Using Math.Max, because a rounding error creates the occasional negative value.
            if (Randomiser.ProbabilityCheck(Math.Max(damage * 0.2, 0.01)))
            {
                switch (type)
                {
                    case (EntityEffectType.EmpDamage):
                        OperationalCondition = SystemCondition.Neutralized;
                        break;

                    case (EntityEffectType.PhysicalDamage):
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

        public IEnumerable<OperateSystemAction> ActionsInRange(Dictionary<HexReactor, List<OperateSystemAction>> dict)
        {
            //if we can't operate the system, return no actions
            if (m_containingEntity.CurrentEnergy < Template.EnergyCost)
            {
                return new OperateSystemAction[0];
            }
            return TargetsInRange().Select(targetedHex => CreateAction(targetedHex, dict));
        }

        #endregion public methods

        #region private methods

        public bool TargetingCheck(HexReactor targetedHex)
        {
            return ((Template.PossibleTargets.HasFlag(TargetingType.EmptyHexes) && targetedHex.Content == null) ||
                    (Template.PossibleTargets.HasFlag(TargetingType.Enemy) && targetedHex.Content != null && targetedHex.Content.Loyalty != m_containingEntity.Loyalty) ||
                    (Template.PossibleTargets.HasFlag(TargetingType.Friendly) && targetedHex.Content != null && targetedHex.Content.Loyalty == m_containingEntity.Loyalty));
        }

        private OperateSystemAction CreateAction(HexReactor targetedHex, Dictionary<HexReactor, List<OperateSystemAction>> dict)
        {
            var list = dict.TryGetOrAdd(targetedHex, () => new List<OperateSystemAction>());
            Assert.EqualOrLesser(list.Count, 6, "Too many subsystems");

            var operation = new OperateSystemAction(m_containingEntity, Effect, Template, targetedHex);
            if (operation.NecessaryConditions())

                list.Add(operation);
            return operation;
        }

        private IEnumerable<HexReactor> TargetsInRange()
        {
            var hex = m_containingEntity.Hex;
            if (Template.MaxRange == 0)
            {
                return new[] { hex };
            }

            var unobstructedShot = Template.DeliveryMethod == DeliveryMethod.Unobstructed;

            if (Template.PossibleTargets.HasFlag(TargetingType.EmptyHexes))
            {
                return hex.RaycastAndResolveHexes(
                    Template.MinRange,
                    Template.MaxRange,
                    TargetingCheck,
                    true,
                    targetedHex => !unobstructedShot &&
                        targetedHex.Content != null &&
                        !targetedHex.Equals(m_containingEntity.Hex));
            }

            if (unobstructedShot)
            {
                return hex.RaycastAndResolveEntities(Template.MinRange, Template.MaxRange, TargetingCheck, true);
            }

            return hex.RaycastAndResolveEntities(Template.MinRange, Template.MaxRange, TargetingCheck, false);
        }

        #endregion private methods
    }

    #endregion Subsystem
}