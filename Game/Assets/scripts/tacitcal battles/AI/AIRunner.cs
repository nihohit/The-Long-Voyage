using System.Collections.Generic;
using System;
using System.Linq;

#region interfaces

//this delegate is used 
public delegate bool ResultEvaluator();

public interface IActionEvaluator
{
    IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity);
}

public interface IEntityEvaluator
{
    double EvaluateValue(Entity entity);
}

#endregion

#region EvaluatedAction

public class EvaluatedAction : IComparable<EvaluatedAction>
{
    public PotentialAction Action { get; set; }
    public double EvaluatedPriority { get; set; }
    public ResultEvaluator AchievedGoal { get; set; }
    public ResultEvaluator NecessaryConditions { get; set;}

    #region IComparable implementation

    public int CompareTo(EvaluatedAction other)
    {
        return EvaluatedPriority.CompareTo(other.EvaluatedPriority);
    }

    #endregion
}

#endregion

#region AIRunner

public class AIRunner
{
    #region private fields
    //all of the entities that are controlled by this AI
    protected List<ActiveEntity> m_controlledEntities;

    //the evaluator which assigns a value to each potential action
    private IActionEvaluator m_actionEvaluator;

    // where the evaluated actions are stored by order
    private IPriorityQueue<EvaluatedAction> m_prioritizedActions;

    #endregion

    #region constructor

    public AIRunner(IEnumerable<ActiveEntity> controlledEntities, IActionEvaluator evaluator)
    {
        m_controlledEntities = controlledEntities.ToList();
        m_actionEvaluator = evaluator;
        m_prioritizedActions = new PriorityQueueB<EvaluatedAction>();
    }

    #endregion

    public void Act()
    {
        EvaluateActions();
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
                    EvaluateActions();
                }
            }
        }
    }

    private void EvaluateActions()
    {
        //remove all destroyed entities
        m_controlledEntities = m_controlledEntities.Where(entity => !entity.Destroyed()).ToList();
        m_controlledEntities.SelectMany(ent => m_actionEvaluator.EvaluateActions(ent))
            .ForEach(action => m_prioritizedActions.Push(action));
    }
}

#endregion


