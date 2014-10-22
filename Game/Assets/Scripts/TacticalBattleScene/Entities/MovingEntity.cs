using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.TacticalBattleScene.PathFinding;
using Assets.Scripts.UnityBase;
using System;
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
        private const float s_speedModifier = 6;

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

        #region public methods

        public override void Update()
        {
            base.Update();
            if (IsMoving())
            {
                var hexObject = ObjectOnPoint(transform.position, UnityEngine.LayerMask.NameToLayer("Hexes"));
                Assert.NotNull(hexObject, "hexObject");
                var hex = hexObject.GetComponent<HexReactor>();
                Assert.NotNull(hex, "hex");
                if (!hex.Equals(Hex))
                {
                    hex.Content = this;
                }
            }
        }

        public void Move(IEnumerable<HexReactor> m_path, Action callback)
        {
            var lastHex = m_path.Last();
            BeginMove(m_path.Select(hex =>
                new MoveOrder(hex.Position,
                hex == lastHex ? callback : () => { })),
                Template.MaxSpeed * s_speedModifier, true);
        }

        #endregion public methods

        #region overrides

        // actions are also all potential movement targets
        protected override IEnumerable<PotentialAction> ComputeActions()
        {
            var baseActions = base.ComputeActions();
            var possibleHexes = AStar.FindAllAvailableHexes(Hex, AvailableSteps, Template.MovementMethod);
            List<PotentialAction> movementActions = possibleHexes.Values.Select(movement => (PotentialAction)movement).ToList();
            var shuffledActions = movementActions.Shuffle().ToList();
            return baseActions.Union(shuffledActions);
        }

        // compute moves at start of turn
        public override bool StartTurn()
        {
            if (base.StartTurn())
            {
                AvailableSteps = Template.MaxSpeed;
                return true;
            }
            AvailableSteps = 0;
            return false;
        }

        public override string FullState()
        {
            return "{0} movement {1}/{2}".FormatWith(base.FullState(), AvailableSteps, Template.MaxSpeed);
        }

        #endregion overrides
    }

    #endregion MovingEntity
}