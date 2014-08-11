using System;
using System.Collections.Generic;
using System.Linq;
using Assets.scripts.Base;
using Assets.scripts.UnityBase;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    /// <summary>
    /// A script wrapper for hexes, to interact with Unity.
    /// Contains all the relevant markers which might cover it.
    /// </summary>
    public class HexReactor : SimpleButton
    {
        #region private fields

        // ensures that the mouse over action will still be activated when displaying commands on the hex.
        private Action m_setMouseOverAction;

        // markers
        private IUnityMarker m_movementPathMarker;

        private IUnityMarker m_fogOfWarMarker;
        private IUnityMarker m_radarBlipMarker;
        private IUnityMarker m_targetMarker;

        // Only a single hex reactor can be selected at any time
        private static MarkerScript s_selected;

        // Only a single hex is hovered over at a single time
        private static HexReactor m_currentHoveredOverHex;

        // The actions that each entity can operate on this hex
        private Dictionary<TacticalEntity, List<OperateSystemAction>> m_orders = new Dictionary<TacticalEntity, List<OperateSystemAction>>();

        // amount of commands currently on display
        private int m_displayCommandsAmount;

        #endregion private fields

        #region properties

        // The hex the reactor is wrapping
        public Hex MarkedHex { get; set; }

        // ensures that the mouse over action will still be activated when displaying commands on the hex.
        public override Action OnMouseOverAction
        {
            get
            {
                return () =>
                {
                    m_setMouseOverAction();
                    DisplayCommands();
                };
            }

            set
            {
                m_setMouseOverAction = value;
            }
        }

        #endregion properties

        #region public methods

        // constructor
        public HexReactor()
        {
            base.ClickableAction = CheckIfClickIsOnUI(() => TacticalState.SelectedHex = this);
            m_setMouseOverAction = () => { };
        }

        #region markers

        public void RemoveMovementMarker()
        {
            RemoveMarker(m_movementPathMarker);
        }

        public void DisplayMovementMarker()
        {
            m_movementPathMarker = AddAndDisplayMarker(m_movementPathMarker, "PathMarker");
        }

        public void RemoveFogOfWarMarker()
        {
            RemoveMarker(m_fogOfWarMarker);
            RemoveRadarBlipMarker();
            if (MarkedHex.Content != null)
            {
                MarkedHex.Content.Reactor.Mark();
            }
        }

        public void DisplayFogOfWarMarker()
        {
            m_fogOfWarMarker = AddAndDisplayMarker(m_fogOfWarMarker, "FogOfWar");
            if (MarkedHex.Content != null)
            {
                MarkedHex.Content.Reactor.Unmark();
            }
        }

        public void RemoveRadarBlipMarker()
        {
            RemoveMarker(m_radarBlipMarker);
        }

        public void DisplayRadarBlipMarker()
        {
            m_radarBlipMarker = AddAndDisplayMarker(m_radarBlipMarker, "RadarBlip");
        }

        public void RemoveTargetMarker()
        {
            RemoveMarker(m_targetMarker);
        }

        // checks if the currently selected entity targets this hex
        // if so, removes the targeting marker.
        public void RemoveTargetMarker(OperateSystemAction action)
        {
            if (TacticalState.SelectedHex == null)
            {
                RemoveMarker(m_targetMarker);
                return;
            }
            var Entity = TacticalState.SelectedHex.MarkedHex.Content as TacticalEntity;
            if (Entity == null)
            {
                RemoveMarker(m_targetMarker);
                return;
            }
            List<OperateSystemAction> actions = null;
            if (Entity != null && m_orders.TryGetValue(Entity, out actions))
            {
                actions.Remove(action);
                m_displayCommandsAmount = actions.Count;
                if (actions.Count == 0)
                {
                    RemoveMarker(m_targetMarker);
                }
            }
        }

        public void DisplayTargetMarker()
        {
            m_targetMarker = AddAndDisplayMarker(m_targetMarker, "targetMarker");
        }

        #endregion markers

        public static void Init()
        {
            s_selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
            s_selected.Unmark();
        }

        // Select this hex, and if there's an active, selectable entity in it, display all of its possible actions
        public void Select()
        {
            //Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex));
            s_selected.Mark(this.transform.position);
            ActionCheck().ForEach(action => action.DisplayAction());
        }

        // Unselect the hex, and remove all action markers
        public void Unselect()
        {
            //Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex));
            s_selected.Unmark();
            var actions = ActionCheck();
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    action.RemoveDisplay();
                    action.TargetedHex.Reactor.RemoveTargetMarker();
                }
            }
        }

        public void AddCommands(TacticalEntity Entity, List<OperateSystemAction> list)
        {
            // assert that all existing orders are destroyed
            Assert.AssertConditionMet(!m_orders.ContainsKey(Entity) || m_orders[Entity].None(order => !order.Destroyed), "Existing orders weren't destroyed");
            m_orders[Entity] = list;
        }

        public void StartTurn()
        {
            m_orders.Clear();
            RemoveMarker(m_targetMarker);
        }

        public void DisplayCommands()
        {
            DisplayCommands(false);
        }

        public void DisplayCommands(bool forceUpdate)
        {
            // Don't do anything if this is already the current hovered over and there's no need to update
            if (!forceUpdate && m_currentHoveredOverHex == this) return;

            // remove the display from the other hovered over hex and set this as the hovered over hex
            if (m_currentHoveredOverHex != null)
            {
                m_currentHoveredOverHex.RemoveCommands();
            }
            m_currentHoveredOverHex = this;

            // If a hex is selected
            if (TacticalState.SelectedHex != null && m_orders.Any())
            {
                List<OperateSystemAction> actions = null;
                var Entity = TacticalState.SelectedHex.MarkedHex.Content as TacticalEntity;

                // if there are any orders to display, display them
                if (Entity != null && m_orders.TryGetValue(Entity, out actions))
                {
                    DisplayCommands(actions);
                }
            }
        }

        public void RemoveCommands()
        {
            if (TacticalState.SelectedHex != null)
            {
                List<OperateSystemAction> actions = null;
                var Entity = TacticalState.SelectedHex.MarkedHex.Content as TacticalEntity;
                if (Entity != null && m_orders.TryGetValue(Entity, out actions))
                {
                    m_displayCommandsAmount = 0;
                    foreach (var action in actions.Where(command => !command.Destroyed))
                    {
                        action.RemoveDisplay();
                    }
                }
            }
        }

        #endregion public methods

        #region private methods

        private void Start()
        {
            DisplayFogOfWarMarker();
        }

        // display commands in a circle around the targeted hex
        private void DisplayCommands(IEnumerable<OperateSystemAction> actions)
        {
            var activeCommands = actions.Where(command => !command.Destroyed).Materialize();
            var commandCount = activeCommands.Count();
            if (m_displayCommandsAmount != commandCount)
            {
                m_displayCommandsAmount = commandCount;
                Vector2 displayOffset = default(Vector2);
                var size = ((CircleCollider2D)this.collider2D).radius;

                int i = 0;
                foreach (var action in activeCommands)
                {
                    i++;
                    switch (i)
                    {
                        case (0):
                            displayOffset = new Vector2(-(size * 2 / 3), 0);
                            break;

                        case (1):
                            displayOffset = new Vector2(-(size / 2), (size * 2 / 3));
                            break;

                        case (2):
                            displayOffset = new Vector2((size / 2), (size * 2 / 3));
                            break;

                        case (3):
                            displayOffset = new Vector2(size * 2 / 3, 0);
                            break;

                        case (4):
                            displayOffset = new Vector2(size / 2, -(size * 2 / 3));
                            break;

                        case (5):
                            displayOffset = new Vector2(-(size / 2), -(size * 2 / 3));
                            break;
                    }

                    action.DisplayAction((Vector2)this.transform.position + displayOffset);
                }
            }
        }

        //returns null if can't return actions, otherwise returns all available actions
        private IEnumerable<PotentialAction> ActionCheck()
        {
            var Entity = MarkedHex.Content as ActiveEntity;
            if (Entity == null || Entity.Loyalty != TacticalState.CurrentTurn)
            {
                return null;
            }

            return Entity.Actions.Materialize();
        }

        private void RemoveMarker(IUnityMarker marker)
        {
            if (marker != null)
            {
                marker.Unmark();
            }
        }

        private IUnityMarker AddAndDisplayMarker(IUnityMarker marker, string markerName)
        {
            if (marker == null)
            {
                marker = ((GameObject)Instantiate(Resources.Load(markerName), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
            }
            marker.Mark(transform.position);
            return marker;
        }

        #endregion private methods
    }
}