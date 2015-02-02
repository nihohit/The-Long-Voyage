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
        IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity, IEnumerable<EntityReactor> entitiesSeenByTeam);

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
        private readonly IActionEvaluator m_actionEvaluator;

        // where the evaluated actions are stored by order
        private readonly IPriorityQueue<EvaluatedAction> m_prioritizedActions;

        private IEnumerable<ActiveEntity> m_controlledEntities;
        private EvaluatedAction m_currentAction;

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
            m_controlledEntities = controlledEntities;
            m_actionEvaluator.Clear();
            EvaluateActions();
            NextAction();
        }

        /// <summary>
        /// combines all seen entities, and send them to all cotrolled entities to evaluate their actions.
        /// </summary>
        private void EvaluateActions()
        {
            Debug.Log("Evaluating actions");
            m_prioritizedActions.Clear();
            m_controlledEntities.ForEach(ent => ent.ResetActions());
            var loyalty = m_controlledEntities.First().Loyalty;
            var entitiesSeen = Enumerable.Empty<EntityReactor>();
            entitiesSeen = m_controlledEntities.Aggregate(entitiesSeen, (current, ent) => current.Union(ent.SeenHexes.Select(hex => hex.Content)
                .Where(entity => entity != null && entity.Loyalty != loyalty)));
            entitiesSeen = entitiesSeen.Distinct();
            m_controlledEntities.SelectMany(ent => m_actionEvaluator.EvaluateActions(ent, entitiesSeen))
                .ForEach(action => m_prioritizedActions.Push(action));
        }

        private void NextAction()
        {
            if (m_currentAction != null && m_currentAction.AchievedGoal())
            {
                EvaluateActions();
            }

            m_currentAction = m_prioritizedActions.Peek();

            if (m_currentAction == null || m_currentAction.EvaluatedPriority <= 0)
            {
                TacticalState.StartTurn();
                return;
            }

            m_prioritizedActions.Pop();

            if (m_currentAction.NecessaryConditions())
            {
                m_currentAction.Action.Callback = NextAction;
                m_currentAction.Action.Commit();
            }
            else
            {
                NextAction();
            }
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

        private readonly IEntityEvaluator m_entityEvaluator;

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
        public IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity, IEnumerable<EntityReactor> entitiesSeenByTeam)
        {
            // initiate relevant information
            var potentialTargets = entitiesSeenByTeam.Where(ent => m_entityEvaluator.EvaluateValue(ent) > 0);
            var minRange = 10000;
            var maxRange = 0;

            foreach (var system in actingEntity.Systems.Where(system => system.Operational()))
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

            // evaluate each action. We're shuffling the actions so that there will be no preference based on order of examination.
            foreach (var action in actingEntity.Actions)
            {
                // TODO - possible to create an AI usage hint enumerator, which will say whether a given system should be used on friendlies or enemies, weakend or strong, etc.
                var evaluatedAction = new EvaluatedAction { Action = action };
                var systemAction = action as OperateSystemAction;
                var movementAction = action as MovementAction;

                if (systemAction != null)
                {
                    // TODO - implicit assumption that all system actions are against entities.
                    var target = systemAction.TargetedHex.Content;
                    if (target != null && target.Loyalty != Loyalty.Inactive)
                    {
                        evaluatedAction.EvaluatedPriority += EvaluateSystemEffect(systemAction.System.Template, target);

                        //Debug.Log("Action {0} valued as {1}".FormatWith(systemAction.Name, evaluatedAction.EvaluatedPriority));
                    }

                    evaluatedAction.NecessaryConditions = () => !actingEntity.Destroyed() && !target.Destroyed();

                    evaluatedAction.AchievedGoal = CreateAchievedGoal(systemAction);
                }
                else if (movementAction != null)
                {
                    var targetHex = movementAction.TargetedHex;

                    // in scouting mode, just go to the most distant hex
                    if (potentialTargets.None(target => target.Loyalty != actingEntity.Loyalty))
                    {
                        // TODO - a better solution would choose a hex by how many new hexes can be seen from it
                        evaluatedAction.EvaluatedPriority = movementAction.TargetedHex.Distance(actingEntity.Hex);
                    }
                    else
                    {
                        // evaluate by targets and what can be done to them
                        // the value of a hex is compared to that of the current location.
                        // TODO - evaluate hex effects
                        evaluatedAction.EvaluatedPriority = EvaluateHexValue(movementAction.TargetedHex, potentialTargets, movingEntity, minRange, maxRange) - currentHexValue;
                    }
                    evaluatedAction.NecessaryConditions = () => targetHex.Content == null;
                    evaluatedAction.AchievedGoal = () => true;
                }
                else
                {
                    throw new UnreachableCodeException("The action {0} should be of a known kind.".FormatWith(action));
                }

                yield return evaluatedAction;
            }
        }

        public void Clear()
        {
            m_entityEvaluator.Clear();
        }

        #endregion IActionEvaluator implementation

        private ResultEvaluator CreateAchievedGoal(OperateSystemAction systemAction)
        {
            var target = systemAction.TargetedHex.Content;
            return () =>
            {
                m_entityEvaluator.UpdateValue(target);
                //Debug.Log("{0} achieved goal? {1} is destroyed? {2}".FormatWith(systemAction, target, target.Destroyed()));
                return target.Destroyed();
            };
        }

        // evaluates a hex based on the value of the systems that can be used from it on targets,
        // or proximity to targets.
        private double EvaluateHexValue(HexReactor evaluatedHex, IEnumerable<EntityReactor> potentialTargets, MovingEntity movingEntity, int minRange, int maxRange)
        {
            var result = 0.0;

            foreach (var target in potentialTargets)
            {
                var distance = evaluatedHex.Distance(target.Hex);
                if (distance >= minRange && distance <= maxRange)
                {
                    result += movingEntity.Systems.Where(system => system.Operational())
                        .Where(system => evaluatedHex.CanAffect(target.Hex, system.Template.DeliveryMethod, minRange, maxRange))
                        .Sum(system => EvaluateSystemEffect(system.Template, target));
                }
                if (result == 0)
                {
                    //TODO - time complexity could be reduced significantly by a. feeding astar some heuristic or b. replacing a star with a heuristic
                    result += m_entityEvaluator.EvaluateValue(target) / AStar.FindPathCost(
                        evaluatedHex, target.Hex, new AStarConfiguration(movingEntity.Template.MovementMethod, hex => 0));
                }
            }
            //Debug.Log("Hex {0} valued as {1}".FormatWith(evaluatedHex, result));
            return result;
        }

        // evaluate the value of a system on a specific target
        private double EvaluateSystemEffect(SubsystemTemplate system, EntityReactor target)
        {
            return (m_entityEvaluator.EvaluateValue(target) + system.EffectStrength) / (system.EnergyCost + system.HeatGenerated);
        }
    }

    #endregion SimpleEvaluator

    #region IEntityEvaluator

    public interface IEntityEvaluator
    {
        double EvaluateValue(EntityReactor entity);

        void UpdateValue(EntityReactor entity);

        void Clear();
    }

    public class SimpleEntityEvaluator : IEntityEvaluator
    {
        private readonly Dictionary<EntityReactor, double> m_entitiesValue = new Dictionary<EntityReactor, double>();

        public double EvaluateValue(EntityReactor entity)
        {
            return (entity.Loyalty == Loyalty.Inactive) ? 0 : EvalueAndAddActiveEntity((ActiveEntity)entity);
        }

        public void UpdateValue(EntityReactor entity)
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
            //Debug.Log("Mech {0} is of value {1}".FormatWith(activeEntity.FullState(), systemsValue + healthValue));
            return value;
        }

        private double EvalueAndAddActiveEntity(ActiveEntity entity)
        {
            return m_entitiesValue.TryGetOrAdd(entity, () => EvaluateActiveEntity(entity));
        }
    }

    #endregion IEntityEvaluator
}