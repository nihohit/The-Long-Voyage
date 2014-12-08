using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    #region ActiveEntity

    /// <summary>
    /// Active entities have subsystems that they can operate, the ability to see & detect other entities,
    /// and several additional properties which are affected by those systems and abilities.
    /// </summary>
    public class ActiveEntity : EntityReactor
    {
        #region constructor

        public virtual void Init(SpecificEntity entity, Loyalty loyalty, IEnumerable<SubsystemTemplate> systems)
        {
            Assert.EqualOrGreater(entity.Template.SystemSlots, systems.Count(), "more systems than system slots.");
            Assert.IsNull(m_systems, "m_systems", "Entity was already initialised.");
            Init(entity, loyalty);
            m_systems = systems.Where(template => template != null).Select(template => new Subsystem(template, this));
            CurrentEnergy = Template.MaxEnergy;
            m_tempMaxEnergy = Template.MaxEnergy;
            Shield = Template.MaxShields;
        }

        #endregion constructor

        #region private fields

        private HashSet<HexReactor> m_detectedHexes;

        private IEnumerable<Subsystem> m_systems;

        private IEnumerable<PotentialAction> m_actions;

        // an entity's energy capacity can be temporarily diminished, for example by EMP weapons.
        private double m_tempMaxEnergy;

        // checks whether a mech shutdown in the last turn. If a mech shuts down two turns in a row it is destroyed
        // TODO - do we want to keep this?
        private bool m_wasShutDown;

        #endregion private fields

        #region Properties

        public IEnumerable<PotentialAction> Actions
        {
            get { return m_actions ?? (m_actions = ComputeActions().Materialize()); }
        }

        public double CurrentEnergy { get; set; }

        public HashSet<HexReactor> SeenHexes { get; private set; }

        public double Shield { get; private set; }

        public double CurrentHeat { get; set; }

        public IEnumerable<Subsystem> Systems { get { return m_systems.Where(system => system.Operational()); } }

        #endregion Properties

        #region public methods

        // destroy all actions
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

        // update all hexes seen by this entity
        // Unsee al seen hexes which are no longer seen, undetect all detected hexes which are no longer detected,
        // and mark as seen & detected newly seen & detected hexes.
        public void SetSeenHexes()
        {
            var whatTheEntitySeesNow = FindSeenHexes();
            var whatTheEntitySeesNowInRadar = FindRadarHexes().Except(whatTheEntitySeesNow);
            //Debug.Log("{0} is setting seen hexes".FormatWith(Name));

            if (SeenHexes != null)
            {
                var whatTheEntitySeesNowSet = new HashSet<HexReactor>(whatTheEntitySeesNow);
                var whatTheEntitySeesNowInRadarSet = new HashSet<HexReactor>(whatTheEntitySeesNowInRadar);
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
            SeenHexes = new HashSet<HexReactor>(whatTheEntitySeesNow);
            m_detectedHexes = new HashSet<HexReactor>(whatTheEntitySeesNowInRadar);
        }

        // reset seeing status
        public void ResetSeenHexes()
        {
            SeenHexes = null;
            m_detectedHexes = null;
            SetSeenHexes();
        }

        // all preparations an entity does at the beginning of its turn
        public virtual bool StartTurn()
        {
            //Debug.Log("Atstart of turn: {0}".FormatWith(FullState()));
            ResetActions();
            ResetSeenHexes();
            CurrentEnergy = m_tempMaxEnergy;
            CurrentHeat = Math.Max(CurrentHeat - Template.HeatLossRate, 0);

            // an entity shutsdown if its temp max energy is negative, or it overheats
            if (m_tempMaxEnergy <= 0 || CurrentHeat >= Template.MaxHeat)
            {
                Debug.Log("{0} shuts down.".FormatWith(FullState()));
                // reset state & mark as shutdown for the turn
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

        protected override void Destroy()
        {
            base.Destroy();
            m_actions.ForEach(action => action.Destroy());
            m_actions = null;
            m_detectedHexes.Clear();
            m_systems = null;
        }

        private IEnumerable<HexReactor> FindSeenHexes()
        {
            //TODO - we might be able to make this somewhat more efficient by combining the sight & radar raycasts, but we should first make sure that it is needed.
            return Hex.RaycastAndResolveHexes(
                0,
                Template.SightRange,
                hex => true,
                true,
                hex =>
                    (hex.Content != null &&
                     hex.Content.Template.Visuals.HasFlag(VisualProperties.BlocksSight)));
        }

        private IEnumerable<HexReactor> FindRadarHexes()
        {
            var inactiveRadarVisibleEntityMarkers = TacticalState.RadarVisibleEntities.Where(ent => !ent.enabled);
            inactiveRadarVisibleEntityMarkers.ForEach(marker => marker.GetComponent<Collider2D>().enabled = true);
            var results = Hex.RaycastAndResolveEntities(0, Template.RadarRange, (hex) => hex.Content != null, true);
            inactiveRadarVisibleEntityMarkers.ForEach(marker => marker.GetComponent<Collider2D>().enabled = false);
            return results;
        }

        // internal damage to an active entity can cause heat, reduce energy levels & damage subsystems.
        protected override void InternalDamage(double damage, EntityEffectType damageType)
        {
            var heatDamage = 0.0;
            var physicalDamage = 0.0;
            var energyDamage = 0.0;

            switch (damageType)
            {
                case EntityEffectType.IncendiaryDamage:
                    heatDamage = damage / 2;
                    physicalDamage = damage / 2;
                    break;

                case EntityEffectType.PhysicalDamage:
                    physicalDamage = damage;
                    break;

                case EntityEffectType.EmpDamage:
                    energyDamage = damage;
                    break;

                case EntityEffectType.HeatDamage:
                    heatDamage = damage;
                    break;
            }

            CurrentHeat += heatDamage;
            m_tempMaxEnergy -= energyDamage;

            if (m_systems.Any(system => system.Operational()))
            {
                m_systems.Where(system => system.Operational()).ChooseRandomValue().Hit(damageType, physicalDamage + energyDamage);
            }

            base.InternalDamage(physicalDamage, EntityEffectType.PhysicalDamage);
        }

        // find all potential targets for all operational subsystems
        protected virtual IEnumerable<PotentialAction> ComputeActions()
        {
            Debug.Log("{0} is computing actions".FormatWith(FullState()));
            if (m_wasShutDown)
            {
                return new PotentialAction[0];
            }

            var dict = new Dictionary<HexReactor, List<OperateSystemAction>>();
            var results = m_systems.Where(system => system.Operational())
                .SelectMany(system => system.ActionsInRange(dict)).Materialize();

            foreach (var hex in dict.Keys)
            {
                hex.AddCommands(this, dict[hex]);
            }

            return results.Select(subsytemAction => (PotentialAction)subsytemAction);
        }

        // external damage to an active entity is mitigated by its shields.
        // different damage types are differently effective against shields
        protected override double ExternalDamage(double strength, EntityEffectType effectType)
        {
            var result = strength;
            if (Shield > 0)
            {
                switch (effectType)
                {
                    case EntityEffectType.PhysicalDamage:
                    case EntityEffectType.EmpDamage:
                        Shield -= strength;
                        strength = -Shield;
                        break;

                    case EntityEffectType.IncendiaryDamage:
                        Shield -= strength / 2;
                        strength = -Shield;
                        break;

                    case EntityEffectType.HeatDamage:
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
}