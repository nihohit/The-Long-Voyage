using Assets.scripts.Base;
using Assets.scripts.LogicBase;
using Assets.scripts.TacticalBattleScene.AI;
using Assets.scripts.InterSceneCommunication;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    public static class TacticalState
    {
        #region fields

        private static HexReactor s_selectedHex;

        private static LinkedList<Loyalty> s_turnOrder;

        private static LinkedListNode<Loyalty> s_currentTurn;

        //for each entity and each hex, the available actions
        private static HashSet<ActiveEntity> s_activeEntities;

        private static IEnumerable<Hex> s_hexes;

        private static Dictionary<Loyalty, IAIRunner> s_nonPlayerTeams;

        //this is needed, since we need to enable all the entities before each radar sweep.
        private static List<TacticalEntity> s_radarableEntity = new List<TacticalEntity>();

        private static List<ActiveEntity> s_destroyedEntities = new List<ActiveEntity>();

        #endregion fields

        #region properties

        public static bool BattleStarted { get; set; }

        public static TacticalTextureHandler TextureManager;

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
                }

                s_selectedHex = value;
                if (s_selectedHex != null)
                    s_selectedHex.Select();
            }
        }

        public static IEnumerable<TacticalEntity> RadarVisibleEntities { get { return s_radarableEntity; } }

        public static Loyalty CurrentTurn { get { return s_currentTurn.Value; } }

        #endregion properties

        #region public methods

        public static void AddRadarVisibleEntity(TacticalEntity ent)
        {
            Assert.AssertConditionMet((ent.Template.Visuals & VisualProperties.AppearsOnRadar) != 0, "Added entity isn't radar visible");
            s_radarableEntity.Add(ent);
        }

        public static void DestroyEntity(TacticalEntity ent)
        {
            if ((ent.Template.Visuals & VisualProperties.AppearsOnRadar) != 0)
            {
                s_radarableEntity.Remove(ent);
            }
            var Entity = ent as ActiveEntity;
            if (Entity != null)
            {
                DestroyEntity(Entity);
            }
            ResetAllActions();
        }

        public static void Init(IEnumerable<ActiveEntity> entities, IEnumerable<Hex> hexes)
        {
            TextureManager = new TacticalTextureHandler();
            BattleStarted = false;
            s_activeEntities = new HashSet<ActiveEntity>(entities);
            entities.ForEach(ent => TextureManager.UpdateEntityTexture(ent));
            var loaylties = entities.Select(ent => ent.Loyalty).Distinct();
            SetTurnOrder(loaylties);
            s_hexes = hexes;
            s_destroyedEntities.Clear();
            s_nonPlayerTeams = new Dictionary<Loyalty, IAIRunner>();
            foreach (var loyalty in loaylties.Where(team => team != Loyalty.Player))
            {
                s_nonPlayerTeams.Add(loyalty, new AIRunner(new SimpleEvaluator(new SimpleEntityEvaluator())));
            }
        }

        public static void StartTurn()
        {
            var thisTurnActiveEntities = s_activeEntities.Where(ent => ent.Loyalty == CurrentTurn);
            thisTurnActiveEntities.ForEach(ent => ent.ResetActions());
            s_currentTurn = s_currentTurn.Next;
            if (s_currentTurn == null)
            {
                s_currentTurn = s_turnOrder.First;
            }
            s_hexes.ForEach(hex => hex.ResetSight());
            Debug.Log("Starting {0}'s turn.".FormatWith(CurrentTurn));
            thisTurnActiveEntities = s_activeEntities.Where(ent => ent.Loyalty == CurrentTurn);
            thisTurnActiveEntities.ForEach(ent => ent.StartTurn());
            SelectedHex = null;
            if (CurrentTurn != Loyalty.Player)
            {
                s_nonPlayerTeams[CurrentTurn].Act(thisTurnActiveEntities);
                StartTurn();
            }
        }

        public static void ResetAllActions()
        {
            s_activeEntities.ForEach(ent => ent.ResetActions());
            SelectedHex = SelectedHex;
        }

        //TODO - remove once we don't have active creation of entities
        public static void AddEntity(TacticalEntity ent)
        {
            var active = ent as ActiveEntity;
            if (active != null)
            {
                s_activeEntities.Add(active);
                TextureManager.UpdateEntityTexture(ent);
            }
            //To refresh the potential actions appearing on screen.
            SelectedHex = SelectedHex;
        }

        #endregion public methods

        #region private method

        private static void DestroyEntity(ActiveEntity ent)
        {
            s_activeEntities.Remove(ent);
            s_destroyedEntities.Add(ent);
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
            GlobalState.BattleSummary = new EndBattleSummary(
                GetSurvivingEntities(),
                GetSalvagedEntities(),
                GetSalvagedEquipment());
            Application.LoadLevel("InventoryScene");
        }

        //returns all player controlled active entities, with all of their undestroyed equipment 
        static IEnumerable<EquippedEntity> GetSurvivingEntities()
        {
            return s_activeEntities.Where(ent => ent.Loyalty == Loyalty.Player)
            //TODO - handle variants
                .Select(ent => new EquippedEntity(new SpecificEntity(ent.Template),
                                                  ent.Systems.Where(system => system.OperationalCondition != SystemCondition.Destroyed)
                                                  .Select(system => system.Template)));
        }

        //return a random sample of destroyed entities as salvage
        static IEnumerable<SpecificEntity> GetSalvagedEntities()
        {
            //TODO - the way they were destroyed should affect the chance of salvage
            //TODO - handle variants
            return s_destroyedEntities.Where(ent => Randomiser.ProbabilityCheck(0.5))
                .Select(ent => new SpecificEntity(ent.Template)); 
        }

        //return a random sample of undestroyed equipment from destroyed entities as salvage
        static IEnumerable<SubsystemTemplate> GetSalvagedEquipment()
        {
            return s_destroyedEntities.SelectMany(ent => ent.Systems)
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