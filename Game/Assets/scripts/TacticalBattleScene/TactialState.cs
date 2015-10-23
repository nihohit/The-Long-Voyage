using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.LogicBase;
using Assets.Scripts.TacticalBattleScene.AI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
	/// <summary>
	/// A static class containing the information of the current battle.
	/// Mostly tracks what happens with the active entities
	/// </summary>
	public static class TacticalState
	{
		#region fields

		// this is needed, since we need to enable all the entities before each radar sweep.
		private static readonly List<EntityReactor> sr_radarableEntity = new List<EntityReactor>();

		private static readonly List<ActiveEntity> sr_destroyedEntities = new List<ActiveEntity>();

		private static HexReactor s_selectedHex;

		private static LinkedList<Loyalty> s_turnOrder;

		private static LinkedListNode<Loyalty> s_currentTurn;

		// for each entity and each hex, the available actions
		private static HashSet<ActiveEntity> s_activeEntities;

		private static IEnumerable<HexReactor> s_hexes;

		private static Dictionary<Loyalty, IAIRunner> s_nonPlayerTeams;

		#endregion fields

		#region properties

		public static bool BattleStarted { get; set; }

		public static TacticalTextureHandler TextureManager { get; private set; }

		// when a hex is selected, mark it as such, and remove the mark from the previously selected hex
		public static HexReactor SelectedHex
		{
			get
			{
				return s_selectedHex;
			}
			set
			{
				if (s_selectedHex != null)
				{
					s_selectedHex.Unselect();
					SelectedActiveEntity = null;
					foreach (var hex in s_hexes)
					{
						hex.RemoveTargetMarker();
					}
				}

				s_selectedHex = value;
				if (s_selectedHex != null)
				{
					s_selectedHex.Select();

					SelectedActiveEntity = s_selectedHex.Content as ActiveEntity;

					if(SelectedActiveEntity!= null)
					{
						SelectedActiveEntity.DisplayActions();
					}
				}
			}
		}

		public static IEnumerable<EntityReactor> RadarVisibleEntities { get { return sr_radarableEntity; } }

		public static Loyalty CurrentTurn { get { return s_currentTurn.Value; } }

		public static PotentialActionsMarker ActionsMarker { get; private set; }

		public static ActiveEntity SelectedActiveEntity { get; private set; }

		#endregion properties

		#region public methods

		// to be called when entities are created
		public static void AddRadarVisibleEntity(EntityReactor ent)
		{
			Assert.AssertConditionMet((ent.Template.Visuals & VisualProperties.AppearsOnRadar) != 0, "Added entity isn't radar visible");
			sr_radarableEntity.Add(ent);
		}

		public static void DestroyEntity(EntityReactor ent)
		{
			if ((ent.Template.Visuals & VisualProperties.AppearsOnRadar) != 0)
			{
				sr_radarableEntity.Remove(ent);
			}
			var entity = ent as ActiveEntity;
			if (entity != null)
			{
				DestroyEntity(entity);
			}
		}

		public static void Init()
		{
			TextureManager = new TacticalTextureHandler();
			BattleStarted = false;
			ActionsMarker = GameObject.Find("PotentialActionsMarker").GetComponent<PotentialActionsMarker>();
			ActionsMarker.gameObject.SetActive(false);
		}

		// initiate a new battle with the relevant information on all active entities
		public static void EnterEntitiesAndHexes(IEnumerable<ActiveEntity> entities, IEnumerable<HexReactor> hexes)
		{
			s_activeEntities = new HashSet<ActiveEntity>(entities);
			sr_radarableEntity.Clear();
			//entities.ForEach(ent => TextureManager.UpdateEntityTexture(ent));
			var loaylties = entities.Select(ent => ent.Loyalty).Distinct();
			SetTurnOrder(loaylties);
			s_hexes = hexes;
			sr_destroyedEntities.Clear();
			s_nonPlayerTeams = new Dictionary<Loyalty, IAIRunner>();
			foreach (var loyalty in loaylties.Where(team => team != Loyalty.Player))
			{
				s_nonPlayerTeams.Add(loyalty, new AIRunner(new SimpleEvaluator(new SimpleEntityEvaluator())));
			}
		}

		// called at the start of each turn
		public static void StartTurn()
		{
			// reset the actions of all entities that could act in the last turn
			var thisTurnActiveEntities = s_activeEntities.Where(ent => ent.Loyalty == CurrentTurn);
			thisTurnActiveEntities.ForEach(ent => ent.ResetActions());

			if (s_currentTurn.Value == Loyalty.Player)
			{
				HexEffect.OperateEffects();
			}

			// pass the turn to the next group
			s_currentTurn = s_currentTurn.Next ?? s_turnOrder.First;

			// reset sight on all hexes
			s_hexes.ForEach(hex => hex.ResetSight());
			Debug.Log("Starting {0}'s turn.".FormatWith(CurrentTurn));

			// start the turn to all of this turn's group's active entities
			thisTurnActiveEntities = s_activeEntities.Where(ent => ent.Loyalty == CurrentTurn).Materialize();
			thisTurnActiveEntities.ForEach(ent => ent.StartTurn());
			SelectedHex = null;

			// if it's not the player's turn, let the computer act and automatically pass the turn
			if (CurrentTurn != Loyalty.Player)
			{
				s_nonPlayerTeams[CurrentTurn].Act(thisTurnActiveEntities);
			}
		}

		// called when all entities need to reevaluate their actions - for example, when something on the map changes.
		public static void ResetAllActions()
		{
			s_activeEntities.ForEach(ent => ent.ResetActions());
			SelectedHex = SelectedHex;
		}

		#endregion public methods

		#region private method

		// remove an entity from the relevant lists and check if the battle ended
		private static void DestroyEntity(ActiveEntity ent)
		{
			s_activeEntities.Remove(ent);
			sr_destroyedEntities.Add(ent);
			//TODO - end battle logic
			if (ent.Loyalty == Loyalty.Player)
			{
				//check if player lost
				if (s_activeEntities.None(entity => entity.Loyalty == Loyalty.Player))
				{
					Debug.Log("Player lost");
					EndBattle();
				}
			}
			if (ent.Loyalty != Loyalty.Player)
			{
				//check if player won
				if (s_activeEntities.None(entity => entity.Loyalty != Loyalty.Player))
				{
					Debug.Log("Player won");
					EndBattle();
				}
			}
		}

		private static void EndBattle()
		{
			HexEffect.Clear();
			GlobalState.Instance.BattleSummary = new EndBattleSummary(
				GetSurvivingEntities(),
				GetSalvagedEntities(),
				GetSalvagedEquipment());
			Application.LoadLevel("StrategicMapScene");
		}

		// returns all player controlled active entities, with all of their undestroyed equipment
		private static IEnumerable<EquippedEntity> GetSurvivingEntities()
		{
			return s_activeEntities.Where(ent => ent.Loyalty == Loyalty.Player)
				// TODO - handle variants
				.Select(ent => new EquippedEntity(
					new SpecificEntity(ent.Template.Name),
					ent.Systems.Where(system => system.OperationalCondition != SystemCondition.Destroyed)
						.Select(system => system.Template.Name)));
		}

		// return a random sample of destroyed entities as salvage
		private static IEnumerable<SpecificEntity> GetSalvagedEntities()
		{
			Debug.Log("{0} entities were destroyed".FormatWith(sr_destroyedEntities.Count));
			// TODO - the way they were destroyed should affect the chance of salvage
			// TODO - handle variants
			return sr_destroyedEntities.Where(ent => Randomiser.ProbabilityCheck(0.5))
				.Select(ent => new SpecificEntity(ent.Template.Name));
		}

		// return a random sample of undestroyed equipment from destroyed entities as salvage
		private static IEnumerable<SubsystemTemplate> GetSalvagedEquipment()
		{
			Debug.Log("{0} systems are salvageable".FormatWith(
				sr_destroyedEntities.SelectMany(ent => ent.Systems).Count(
					system => system.OperationalCondition != SystemCondition.Destroyed)));
			return sr_destroyedEntities.SelectMany(ent => ent.Systems)
				.Where(system => system.OperationalCondition != SystemCondition.Destroyed)
				.Where(system => Randomiser.ProbabilityCheck(0.5)).Select(system => system.Template);
		}

		private static void SetTurnOrder(IEnumerable<Loyalty> players)
		{
			s_turnOrder = new LinkedList<Loyalty>(players);
			s_currentTurn = s_turnOrder.First;
		}

		#endregion private method
	}
}