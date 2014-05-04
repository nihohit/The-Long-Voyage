using System.Collections.Generic;
using System;
using System.Linq;

public delegate bool ResultEvaluator();

public interface IActionEvaluator
{
    EvaluatedAction EvaluateAction(PotentialAction action);
}

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

public class AI
{
    #region private fields
    //all of the entities that are controlled by this AI
    private List<ActiveEntity> m_controlledEntities;

    //the evaluator which assigns a value to each potential action
    private IActionEvaluator m_actionEvaluator;

    // where the evaluated actions are stored by order
    private IPriorityQueue<EvaluatedAction> m_prioritizedActions;

    #endregion

    #region constructor

    public AI(IEnumerable<ActiveEntity> controlledEntities, IActionEvaluator evaluator)
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
        m_controlledEntities.SelectMany(ent => ent.Actions)
            .ForEach(action => m_prioritizedActions.Push(m_actionEvaluator.EvaluateAction(action)));
    }

}


