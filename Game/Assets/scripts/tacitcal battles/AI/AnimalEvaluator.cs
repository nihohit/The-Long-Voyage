using System.Collections.Generic;
using System.Linq;

/**there's a hidden assumption here that no AI of this type will have
 * a system which affects empty hexes. Since this is supposed to be 
 * only for the simplest of AIs, this assumption should hold. */
public class AnimalEvaluator : IActionEvaluator 
{
    private IEntityEvaluator m_entityEvaluator;
    private IEnumerable<ActiveEntity> m_controlledEntities;

    #region constructor

    public AnimalEvaluator(IEnumerable<ActiveEntity> controlledEntities, IEntityEvaluator entityEvaluator)
    {
        m_entityEvaluator = entityEvaluator;
        m_controlledEntities = controlledEntities;
    }

    #endregion

    #region IActionEvaluator implementation

    /**evaluates a system action based on the importance of its target,
     * and movement commands based on nearness to potential targets. 
     * If no potential targets are in sight, randomly roam. */
    public IEnumerable<EvaluatedAction> EvaluateActions(ActiveEntity actingEntity)
    {
        var potentialTargets = actingEntity.SeenHexes.Select(hex => hex.Content).Where(ent => ent != null && ent.Loyalty != Loyalty.Neutral)
            .OrderBy(ent => m_entityEvaluator.EvaluateValue(ent)).Where(ent => m_entityEvaluator.EvaluateValue(ent) > 0);
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
                evaluatedAction.EvaluatedPriority = m_entityEvaluator.EvaluateValue(target);
                evaluatedAction.NecessaryConditions = () =>
                {
                    return !actingEntity.Destroyed() && !target.Destroyed();
                };
                evaluatedAction.AchievedGoal = () => 
                {
                    return target.Loyalty == actingEntity.Loyalty || target.Destroyed();
                };
            } else if (movementAction != null)
            {
                var value = 0.0;
                foreach(var target in potentialTargets)
                {
                    value +=  m_entityEvaluator.EvaluateValue(target) / AStar.FindPathCost(
                        movementAction.TargetedHex, target.Hex, new AStarConfiguration(movingEntity.MovementMethod, (Hex hex) => 0));
                }
            }

            yield return evaluatedAction;
        }
    }

    #endregion
}
