using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

#region interfaces

//this delegate is used 
public delegate bool ResultEvaluator();

public interface IActionEvaluator
{
    IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity, IEnumerable<Entity> entitiesSeenByTeam);
}

public interface IAIRunner
{
    void Act(IEnumerable<ActiveEntity> entities);
}

#endregion

#region EvaluatedAction

public class EvaluatedAction : IComparable<EvaluatedAction>
{
    private ResultEvaluator m_necessaryConditions;

    public PotentialAction Action { get; set; }
    public double EvaluatedPriority { get; set; }
    public ResultEvaluator AchievedGoal { get; set; }
    public ResultEvaluator NecessaryConditions { 
        get
        {
            return () => (m_necessaryConditions() && Action.NecessaryConditions());
        }
        set
        {
            m_necessaryConditions = value;
        }}

    #region IComparable implementation

    public int CompareTo(EvaluatedAction other)
    {
        return other.EvaluatedPriority.CompareTo(EvaluatedPriority);
    }

    #endregion
}

#endregion

#region AIRunner

public class AIRunner : IAIRunner
{
    #region private fields

    //the evaluator which assigns a value to each potential action
    private IActionEvaluator m_actionEvaluator;

    // where the evaluated actions are stored by order
    private IPriorityQueue<EvaluatedAction> m_prioritizedActions;

    #endregion

    #region constructor

    public AIRunner(IActionEvaluator evaluator)
    {
        m_actionEvaluator = evaluator;
        m_prioritizedActions = new PriorityQueueB<EvaluatedAction>();
    }

    #endregion

    public void Act(IEnumerable<ActiveEntity> controlledEntities)
    {

        EvaluateActions(controlledEntities);
        while (m_prioritizedActions.Peek() != null)
        {
            var action = m_prioritizedActions.Pop();
            if(action.EvaluatedPriority <= 0)
            {
                break;
            }
            if(action.NecessaryConditions())
            {
                action.Action.Commit();
                if(action.AchievedGoal())
                {
                    m_prioritizedActions.Clear();
                    EvaluateActions(controlledEntities);
                }
            }
        }
    }

    private void EvaluateActions(IEnumerable<ActiveEntity> controlledEntities)
    {
        Debug.Log("Evaluating actions");
        controlledEntities.ForEach(ent => ent.ResetActions());
        var entitiesSeen = Enumerable.Empty<Entity>();
        foreach(var ent in controlledEntities)
        {
            entitiesSeen = entitiesSeen.Union(ent.SeenHexes.Select(hex => hex.Content).Where(entity => entity != null));
        }
        entitiesSeen = entitiesSeen.Distinct();
        controlledEntities.SelectMany(ent => m_actionEvaluator.EvaluateActions(ent, entitiesSeen))
            .ForEach(action => m_prioritizedActions.Push(action));
    }
}

#endregion

#region AnimalEvaluator

/**there's a hidden assumption here that no AI of this type will have
 * a system which affects empty hexes. Since this is supposed to be 
 * only for the simplest of AIs, this assumption should hold. */
public class AnimalEvaluator : IActionEvaluator 
{
    #region fields
    
    private IEntityEvaluator m_entityEvaluator;
    
    #endregion
    
    #region constructor

    public AnimalEvaluator(IEntityEvaluator entityEvaluator)
    {
        m_entityEvaluator = entityEvaluator;
    }
    
    #endregion
    
    #region IActionEvaluator implementation
    
    /**evaluates a system action based on the importance of its target,
     * and movement commands based on nearness to potential targets. 
     * If no potential targets are in sight, randomly roam. */
    public IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity, IEnumerable<Entity> entitiesSeenByTeam)
    {
        var potentialTargets = entitiesSeenByTeam.Where(ent => m_entityEvaluator.EvaluateValue(ent) > 0);
        var movingEntity = actingEntity as MovingEntity;
        foreach (var action in actingEntity.Actions)
        {
            var evaluatedAction = new EvaluatedAction();
            evaluatedAction.Action = action;
            var systemAction = action as OperateSystemAction;
            var movementAction = action as MovementAction;

            if (systemAction != null)
            {
                var target = systemAction.TargetedHex.Content;
                if(target.Loyalty != Loyalty.Inactive)
                {
                    evaluatedAction.EvaluatedPriority = m_entityEvaluator.EvaluateValue(target);
                    evaluatedAction.NecessaryConditions = () =>
                    {
                        return !actingEntity.Destroyed() && !target.Destroyed();
                    };
                    evaluatedAction.AchievedGoal = () => 
                    {
                        return target.Destroyed();
                    };
                    evaluatedAction.EvaluatedPriority = m_entityEvaluator.EvaluateValue(target);
                }
            } 
            else if (movementAction != null)
            {
                evaluatedAction.EvaluatedPriority = 0.0;
                foreach(var target in potentialTargets)
                {
                    evaluatedAction.EvaluatedPriority +=  m_entityEvaluator.EvaluateValue(target) / AStar.FindPathCost(
                        movementAction.TargetedHex, target.Hex, new AStarConfiguration(movingEntity.MovementMethod, (Hex hex) => 0));
                }
            }
            
            yield return evaluatedAction;
        }
    }
    
    #endregion
}

#endregion

#region IEntityEvaluator

public interface IEntityEvaluator
{
    double EvaluateValue(Entity entity);
}

public class SimpleEntityEvaluator : IEntityEvaluator
{
    public double EvaluateValue(Entity entity)
    {
        return (entity.Loyalty == Loyalty.Inactive) ? 0 : 10000;
    }
}

#endregion
