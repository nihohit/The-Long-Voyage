using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    public class Subsystem
    {
        #region fields

        private readonly ActiveEntity r_containingEntity;

        private readonly HexEffectTemplate r_hexEffectTemplate;

        private SystemCondition m_workingCondition;

        private int m_ammo;

        private int m_actionsThisTurn;

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
                // condition most be ordered by order of severity
                if (value > m_workingCondition)
                {
                    m_workingCondition = value;
                }
            }
        }

        public SubsystemTemplate Template { get; private set; }

        public Event OutOfActionsForThisTurn;

        #endregion properties

        #region constructors

        public Subsystem(SubsystemTemplate template, ActiveEntity containingEntity)
        {
            this.r_containingEntity = containingEntity;
            m_workingCondition = SystemCondition.Operational;
            Template = template;
            this.r_hexEffectTemplate = Template.HexEffect;
        }

        #endregion constructors

        #region public methods

        public void Effect(HexReactor targetHex)
        {
            if (Template.Effect != EntityEffectType.None && targetHex.Content != null)
            {
                targetHex.Content.Affect(Template.EffectStrength, Template.Effect);
            }

            if (this.r_hexEffectTemplate != null)
            {
                HexEffect.Create(this.r_hexEffectTemplate, targetHex);
            }
        }

        public void StartTurn()
        {
            Debug.Log("{0} started turn".FormatWith(this.Template.Name));
            m_actionsThisTurn = Template.ActionsPerTurn;
        }

        public void Hit(EntityEffectType type, double damage)
        {
            // TODO - decide on a relevant value.
            // Using Math.Max, because a rounding error creates the occasional negative value.
            if (Randomiser.ProbabilityCheck(Math.Max(damage * 0.2, 0.01)))
            {
                switch (type)
                {
                    case EntityEffectType.EmpDamage:
                        OperationalCondition = SystemCondition.Neutralized;
                        break;

                    case EntityEffectType.PhysicalDamage:
                        OperationalCondition = SystemCondition.Destroyed;
                        break;
                }
            }

            Debug.Log("{0} was hit for {1} {2} damage, it is now {3}".FormatWith(Template.Name, damage, type, OperationalCondition));
        }

        public bool CanOperateNow()
        {
            return Operational() && CanOperate();
        }

        public bool Operational()
        {
            return m_workingCondition == SystemCondition.Operational;
        }

        public IEnumerable<OperateSystemAction> ActionsInRange()
        {
            // if we can't operate the system, return no actions
            Assert.AssertConditionMet(Operational(), "System {0} can't act now".FormatWith(this));
			return TargetsInRange().Select(targetedHex => new OperateSystemAction(this.r_containingEntity, Effect, this, targetedHex));
        }

        public void Act()
        {
            Assert.Greater(m_actionsThisTurn, 0, "No actions left.");

            m_actionsThisTurn--;

            if (this.m_ammo <= 0)
            {
                return;
            }

            --this.m_ammo;
            if (this.m_ammo == 0)
            {
                this.m_workingCondition = SystemCondition.OutOfAmmo;
            }
        }

        #endregion public methods

        #region private methods

        private bool CanOperate()
        {
            return this.r_containingEntity.CurrentEnergy >= Template.EnergyCost && m_actionsThisTurn > 0;
        }

        public bool TargetingCheck(HexReactor targetedHex)
        {
            return (Template.PossibleTargets.HasFlag(TargetingType.EmptyHexes) && targetedHex.Content == null) ||
                    (Template.PossibleTargets.HasFlag(TargetingType.Enemy) && targetedHex.Content != null && targetedHex.Content.Loyalty != this.r_containingEntity.Loyalty) ||
                    (Template.PossibleTargets.HasFlag(TargetingType.Friendly) && targetedHex.Content != null && targetedHex.Content.Loyalty == this.r_containingEntity.Loyalty);
        }

        private IEnumerable<HexReactor> TargetsInRange()
        {
            var hex = this.r_containingEntity.Hex;
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
                        !targetedHex.Equals(this.r_containingEntity.Hex));
            }

            return hex.RaycastAndResolveHexes(Template.MinRange, Template.MaxRange, TargetingCheck, unobstructedShot, hit => false);
        }

        #endregion private methods
    }

    public class SubsysteByTemplateComparer : IEqualityComparer<Subsystem>
    {
        public bool Equals(Subsystem first, Subsystem second)
        {
            return first.Template.Equals(second.Template);
        }

        public int GetHashCode(Subsystem system)
        {
            return system.Template.GetHashCode();
        }
    }
}