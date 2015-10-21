using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.TacticalBattleScene.PathFinding;
using Assets.Scripts.UnityBase;

namespace Assets.Scripts.TacticalBattleScene
{
	#region MovingEntity

	/// <summary>
	/// an entity that can move
	/// </summary>
	public class MovingEntity : ActiveEntity
	{
		private const float c_speedModifier = 6;

		#region properties

		public double AvailableSteps { get; set; }

		#endregion properties

		#region constructor

		public override void Init(SpecificEntity entity, Loyalty loyalty, IEnumerable<SubsystemTemplate> systems)
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

		public void Move(IEnumerable<HexReactor> path, Action callback)
		{
			Assert.NotNullOrEmpty(path, "path");
			var lastHex = path.Last();
			BeginMove(path.Select(hex =>
				new MoveOrder(hex.Position,
				hex == lastHex ? callback : () => { })),
				Template.MaxSpeed * c_speedModifier, true);
		}

		#endregion public methods

		#region overrides

		// actions are also all potential movement targets
		protected override IDictionary<HexReactor, List<PotentialAction>> ComputeActions()
		{
			var dict = base.ComputeActions();
			var possibleHexes = AStar.FindAllAvailableHexes(Hex, AvailableSteps, Template.MovementMethod);

			foreach (var action in possibleHexes)
			{
				dict.TryGetOrAdd(action.Key, () => new List<PotentialAction>()).Add(action.Value);
			}

			return dict;
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