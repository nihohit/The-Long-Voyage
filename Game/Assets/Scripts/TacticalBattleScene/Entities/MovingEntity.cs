using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.TacticalBattleScene.PathFinding;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.TacticalBattleScene
{
    #region MovingEntity

    /// <summary>
    /// an entity that can move
    /// </summary>
    public class MovingEntity : ActiveEntity
    {
        #region properties

        public double AvailableSteps { get; set; }

        #endregion properties

        #region constructor

        public override void Init(SpecificEntity entity, Loyalty loyalty, IEnumerable<Subsystem> systems)
        {
            base.Init(entity, loyalty, systems);
            AvailableSteps = Template.MaxSpeed;
        }

        #endregion constructor

        #region overrides

        // actions are also all potential movement targets
        protected override IEnumerable<PotentialAction> ComputeActions()
        {
            var baseActions = base.ComputeActions();
            var possibleHexes = AStar.FindAllAvailableHexes(Hex, AvailableSteps, Template.MovementMethod);
            return baseActions.Union(possibleHexes.Values.Select(movement => (PotentialAction)movement).Shuffle());
        }

        // compute moves at start of turn
        public override bool StartTurn()
        {
            if (base.StartTurn())
            {
                AvailableSteps = Template.MaxSpeed;
                return true;
            }
            else
            {
                AvailableSteps = 0;
                return false;
            }
        }

        public override string FullState()
        {
            return "{0} movement {1}/{2}".FormatWith(base.FullState(), AvailableSteps, Template.MaxSpeed);
        }

        #endregion overrides
    }

    #endregion MovingEntity
}