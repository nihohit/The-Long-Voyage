using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.TacticalBattleScene.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene.AI
{
    #region interfaces

    // used to estimate if some result passed some kind of condition
    public delegate bool ResultEvaluator();

    public interface IActionEvaluator
    {
        IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity, IEnumerable<TacticalEntity> entitiesSeenByTeam);

        void Clear();
    }

    public interface IAIRunner
    {
        void Act(IEnumerable<ActiveEntity> entities);
    }

    #endregion interfaces

    #region EvaluatedAction

    /// <summary>
    /// An action with an evaluation of its cost, and ways to evaluate if it can run and if it achieved its goal.
    /// When an action achieved its goals all actions of that entity are re-evaluated, so that no action which tries to achieve its goal will run.
    /// </summary>
    public class EvaluatedAction : IComparable<EvaluatedAction>
    {
        private ResultEvaluator m_necessaryConditions;

        public PotentialAction Action { get; set; }

        public double EvaluatedPriority { get; set; }

        public ResultEvaluator AchievedGoal { get; set; }

        public ResultEvaluator NecessaryConditions
        {
            get
            {
                return () => (m_necessaryConditions() && Action.NecessaryConditions());
            }
            set
            {
                m_necessaryConditions = value;
            }
        }

        #region IComparable implementation

        public int CompareTo(EvaluatedAction other)
        {
            return other.EvaluatedPriority.CompareTo(EvaluatedPriority);
        }

        #endregion IComparable implementation
    }

    #endregion EvaluatedAction

    #region AIRunner

    /// <summary>
    /// A basic runner for a group of individual AI entities which share vision, but not tactics.
    /// Always chooses the most evaluated action out of all the possible ones for all entities.
    /// </summary>
    public class AIRunner : IAIRunner
    {
        #region private fields

        //the evaluator which assigns a value to each potential action
        private IActionEvaluator m_actionEvaluator;

        // where the evaluated actions are stored by order
        private IPriorityQueue<EvaluatedAction> m_prioritizedActions;

        #endregion private fields

        #region constructor

        public AIRunner(IActionEvaluator evaluator)
        {
            m_actionEvaluator = evaluator;
            m_prioritizedActions = new PriorityQueue<EvaluatedAction>();
        }

        #endregion constructor

        /// <summary>
        /// repeatedly choose most valuable action.
        /// </summary>
        /// <param name="controlledEntities"></param>
        public void Act(IEnumerable<ActiveEntity> controlledEntities)
        {
            m_actionEvaluator.Clear();
            EvaluateActions(controlledEntities);
            while (m_prioritizedActions.Peek() != null)
            {
                var action = m_prioritizedActions.Pop();
                if (action.EvaluatedPriority <= 0)
                {
                    break;
                }
                if (action.NecessaryConditions())
                {
                    action.Action.Commit();
                    if (action.AchievedGoal())
                    {
                        m_prioritizedActions.Clear();
                        EvaluateActions(controlledEntities);
                    }
                }
            }
        }

        /// <summary>
        /// combines all seen entities, and send them to all cotrolled entities to evaluate their actions.
        /// </summary>
        /// <param name="controlledEntities"></param>
        private void EvaluateActions(IEnumerable<ActiveEntity> controlledEntities)
        {
            //Debug.Log("Evaluating actions");
            controlledEntities.ForEach(ent => ent.ResetActions());
            var loyalty = controlledEntities.First().Loyalty;
            var entitiesSeen = Enumerable.Empty<TacticalEntity>();
            foreach (var ent in controlledEntities)
            {
                entitiesSeen = entitiesSeen.Union(ent.SeenHexes.Select(hex => hex.Content).Where(entity => entity != null && entity.Loyalty != loyalty));
            }
            entitiesSeen = entitiesSeen.Distinct();
            controlledEntities.SelectMany(ent => m_actionEvaluator.EvaluateActions(ent, entitiesSeen))
                .ForEach(action => m_prioritizedActions.Push(action));
        }
    }

    #endregion AIRunner

    #region SimpleEvaluator

    /**there's a hidden assumption here that no AI of this type will have
    * a system which affects empty hexes. Since this is supposed to be
    * only for the simplest of AIs, this assumption should hold. */

    public class SimpleEvaluator : IActionEvaluator
    {
        #region fields

        private IEntityEvaluator m_entityEvaluator;

        #endregion fields

        #region constructor

        public SimpleEvaluator(IEntityEvaluator entityEvaluator)
        {
            m_entityEvaluator = entityEvaluator;
        }

        #endregion constructor

        #region IActionEvaluator implementation

        ///
        /// evaluates a system action based on the importance of its target,
        /// and movement commands based on nearness to potential targets.
        /// If no potential targets are in sight, randomly roam.
        ///
        public IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity, IEnumerable<TacticalEntity> entitiesSeenByTeam)
        {
            //initiate relevant information
            var potentialTargets = entitiesSeenByTeam.Where(ent => m_entityEvaluator.EvaluateValue(ent) > 0);
            var minRange = 10000;
            var maxRange = 0;

            foreach (var system in actingEntity.Systems)
            {
                maxRange = Math.Max(maxRange, system.Template.MaxRange);
                minRange = Math.Min(minRange, system.Template.MinRange);
            }
            var movingEntity = actingEntity as MovingEntity;
            var currentHexValue = 0.0;
            if (movingEntity != null)
            {
                currentHexValue = EvaluateHexValue(actingEntity.Hex, potentialTargets, movingEntity, minRange, maxRange);
            }

            //evaluate each action. We're shuffling the actions so that there will be no preference based on order of examination.
            foreach (var action in actingEntity.Actions)
            {
                //TODO - possible to create an AI usage hint enumerator, which will say whether a given system should be used on friendlies or enemies, weakend or strong, etc.
                var evaluatedAction = new EvaluatedAction();
                evaluatedAction.Action = action;
                var systemAction = action as OperateSystemAction;
                var movementAction = action as MovementAction;

                if (systemAction != null)
                {
                    var target = systemAction.TargetedHex.Content;
                    if (target.Loyalty != Loyalty.Inactive)
                    {
                        evaluatedAction.EvaluatedPriority += EvaluateSystemEffect(systemAction.System, target);
                        evaluatedAction.NecessaryConditions = () =>
                        {
                            return !actingEntity.Destroyed() && !target.Destroyed();
                        };
                        evaluatedAction.AchievedGoal = () =>
                            {
                                m_entityEvaluator.UpdateValue(target);
                                return target.Destroyed();
                            };
                        //Debug.Log("Action {0} valued as {1}".FormatWith(systemAction.Name, evaluatedAction.EvaluatedPriority));
                    }
                }
                else if (movementAction != null)
                {
                    var targetHex = movementAction.TargetedHex;
                    // in scouting mode, just go to the most distant hex
                    if (potentialTargets.None(target => target.Loyalty != actingEntity.Loyalty))
                    {
                        //TODO - a better solution would choose a hex by how many new hexes can be seen from it
                        evaluatedAction.EvaluatedPriority = movementAction.TargetedHex.Distance(actingEntity.Hex);
                    }
                    // evaluate by targets and what can be done to them
                    else
                    {
                        //the value of a hex is compared to that of the current location.
                        evaluatedAction.EvaluatedPriority = EvaluateHexValue(movementAction.TargetedHex, potentialTargets, movingEntity, minRange, maxRange) - currentHexValue;
                    }
                    evaluatedAction.NecessaryConditions = () =>
                    {
                        return targetHex.Content == null;
                    };
                    evaluatedAction.AchievedGoal = () => { return true; };
                }

                yield return evaluatedAction;
            }
        }

        public void Clear()
        {
            m_entityEvaluator.Clear();
        }

        #endregion IActionEvaluator implementation

        // evaluates a hex based on the value of the systems that can be used from it on targets,
        // or proximity to targets.
        private double EvaluateHexValue(Hex evaluatedHex, IEnumerable<TacticalEntity> potentialTargets, MovingEntity movingEntity, int minRange, int maxRange)
        {
            var result = 0.0;

            foreach (var target in potentialTargets)
            {
                var distance = evaluatedHex.Distance(target.Hex);
                if (distance >= minRange && distance <= maxRange)
                {
                    foreach (var system in movingEntity.Systems)
                    {
                        if (evaluatedHex.CanAffect(target.Hex, system.Template.DeliveryMethod, minRange, maxRange))
                        {
                            result += EvaluateSystemEffect(system.Template, target);
                        }
                    }
                }
                if (result == 0)
                {
                    //TODO - time complexity could be reduced significantly by a. feeding astar some heuristic or b. replacing a star with a heuristic
                    result += m_entityEvaluator.EvaluateValue(target) / AStar.FindPathCost(
                        evaluatedHex, target.Hex, new AStarConfiguration(movingEntity.Template.MovementMethod, (Hex hex) => 0));
                }
            }
            //Debug.Log("Hex {0} valued as {1}".FormatWith(evaluatedHex, result));
            return result;
        }

        // evaluate the value of a system on a specific target
        private double EvaluateSystemEffect(SubsystemTemplate system, TacticalEntity target)
        {
            return (m_entityEvaluator.EvaluateValue(target) + system.EffectStrength) / (system.EnergyCost + system.HeatGenerated);
        }
    }

    #endregion SimpleEvaluator

    #region IEntityEvaluator

    public interface IEntityEvaluator
    {
        double EvaluateValue(TacticalEntity entity);

        void UpdateValue(TacticalEntity entity);

        void Clear();
    }

    public class SimpleEntityEvaluator : IEntityEvaluator
    {
        private Dictionary<TacticalEntity, double> m_entitiesValue = new Dictionary<TacticalEntity, double>();

        public double EvaluateValue(TacticalEntity entity)
        {
            return (entity.Loyalty == Loyalty.Inactive) ? 0 : EvalueAndAddActiveEntity((ActiveEntity)entity);
        }

        public void UpdateValue(TacticalEntity entity)
        {
            m_entitiesValue.Remove(entity);
        }

        public void Clear()
        {
            m_entitiesValue.Clear();
        }

        private double EvaluateActiveEntity(ActiveEntity activeEntity)
        {
            var systemsValue = activeEntity.Systems.Where(system => system.Operational()).Sum(system => (system.Template.EffectStrength + system.Template.MaxRange - system.Template.MinRange) / (system.Template.EnergyCost + system.Template.HeatGenerated));
            var healthValue = activeEntity.Shield + activeEntity.Health - activeEntity.CurrentHeat;
            var value = systemsValue / healthValue;
            Debug.Log("Mech {0} is of value {1}".FormatWith(activeEntity.FullState(), systemsValue + healthValue));
            return value;
        }

        private double EvalueAndAddActiveEntity(ActiveEntity entity)
        {
            return m_entitiesValue.TryGetOrAdd(entity, () => EvaluateActiveEntity(entity));
        }
    }

    #endregion IEntityEvaluator
}