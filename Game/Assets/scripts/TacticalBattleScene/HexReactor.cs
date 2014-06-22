using Assets.scripts.Base;
using Assets.scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    public class HexReactor : CircularButton
    {
        #region private fields

        private Action m_setMouseOverAction;
        private MarkerScript m_movementPathMarker;
        private MarkerScript m_fogOfWarMarker;
        private MarkerScript m_radarBlipMarker;
        private MarkerScript m_targetMarker;
        private static MarkerScript s_selected;
        private Dictionary<Entity, List<OperateSystemAction>> m_orders = new Dictionary<Entity, List<OperateSystemAction>>();
        private int m_displayCommands;
        private static HexReactor m_currentHoveredHex;

        #endregion private fields

        #region properties

        public Hex MarkedHex { get; set; }

        public override Action OnMouseOverProperty
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

        public void RemoveTargetMarker(OperateSystemAction action)
        {
            if (TacticalState.SelectedHex == null)
            {
                RemoveMarker(m_targetMarker);
                return;
            }
            var Entity = TacticalState.SelectedHex.MarkedHex.Content as Entity;
            if (Entity == null)
            {
                RemoveMarker(m_targetMarker);
                return;
            }
            List<OperateSystemAction> actions = null;
            if (Entity != null && m_orders.TryGetValue(Entity, out actions))
            {
                actions.Remove(action);
                m_displayCommands = actions.Count;
                if (actions.Count == 0)
                {
                    RemoveMarker(m_targetMarker);
                }
            }
        }

        public void RemoveTargetMarker()
        {
            RemoveMarker(m_targetMarker);
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

        private void Start()
        {
            DisplayFogOfWarMarker();
        }

        public void Select()
        {
            //Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex));
            s_selected.Mark(this.transform.position);
            ActionCheck().ForEach(action => action.DisplayButton());
        }

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

        public void AddCommands(Entity Entity, List<OperateSystemAction> list)
        {
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
            if (!forceUpdate && m_currentHoveredHex == this) return;

            if (m_currentHoveredHex != null)
            {
                m_currentHoveredHex.RemoveCommands();
            }
            m_currentHoveredHex = this;

            if (TacticalState.SelectedHex != null && m_orders.Any())
            {
                List<OperateSystemAction> actions = null;
                var Entity = TacticalState.SelectedHex.MarkedHex.Content as Entity;
                if (Entity != null && m_orders.TryGetValue(Entity, out actions))
                {
                    var activeCommands = actions.Where(command => !command.Destroyed).Materialize();
                    var commandCount = activeCommands.Count();
                    if (m_displayCommands != commandCount)
                    {
                        m_displayCommands = commandCount;
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

                            action.DisplayButton((Vector2)this.transform.position + displayOffset);
                        }
                    }
                }
            }
        }

        public void RemoveCommands()
        {
            if (TacticalState.SelectedHex != null)
            {
                List<OperateSystemAction> actions = null;
                var Entity = TacticalState.SelectedHex.MarkedHex.Content as Entity;
                if (Entity != null && m_orders.TryGetValue(Entity, out actions))
                {
                    m_displayCommands = 0;
                    foreach (var action in actions.Where(command => !command.Destroyed))
                    {
                        action.RemoveDisplay();
                    }
                }
            }
        }

        #endregion public methods

        #region private methods

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

        private void RemoveMarker(MarkerScript marker)
        {
            if (marker != null)
            {
                marker.Unmark();
            }
        }

        private MarkerScript AddAndDisplayMarker(MarkerScript marker, string markerName)
        {
            if (marker == null)
            {
                marker = ((GameObject)Instantiate(Resources.Load(markerName), Vector3.zero, Quaternion.identity)).GetComponent<MarkerScript>();
                marker.internalRenderer = marker.GetComponent<SpriteRenderer>();
            }
            marker.Mark(transform.position);
            return marker;
        }

        #endregion private methods
    }
}