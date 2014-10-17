using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
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
        private Dictionary<EntityReactor, List<OperateSystemAction>> m_orders = new Dictionary<EntityReactor, List<OperateSystemAction>>();

        // amount of commands currently on display
        private int m_displayCommandsAmount;

        //holds all the hexes by their hex-coordinates
        private static Dictionary<Vector2, HexReactor> s_repository = new Dictionary<Vector2, HexReactor>();

        private EntityReactor m_content = null;

        //these shouldn't be touched directly. There's a property for that.
        private int m_seen = 0, m_detected = 0;

        #endregion private fields

        #region properties

        public HexEffect Effects { get; private set; }

        public Biome BiomeType { get; private set; }

        public Vector2 Coordinates { get; private set; }

        public TraversalConditions Conditions { get; set; }

        // what entity stands in the hex.
        // when the content updates, this logic updates moving reactors around and updating sight & actions for the moved entity
        public EntityReactor Content
        {
            get
            {
                return m_content;
            }
            set
            {
                //if the game hasn't started yet
                if (!TacticalState.BattleStarted)
                {
                    m_content = value;
                    m_content.Hex = this;
                    m_content.Mark(Position);
                    return;
                }

                // if an entity moved out of the hex
                if (value == null)
                {
                    Assert.AssertConditionMet((m_content.Destroyed()) ||
                                                (m_content.Hex != null &&
                                                !m_content.Hex.Equals(this)),
                                                "When replaced with a null value, entity should either move to another hex or be destroyed");
                    m_content = value;
                    return;
                }
                Debug.Log("Enter {0} to {1}".FormatWith(value, this));

                Assert.NotEqual(value, m_content, "Entered the same entity to hex {0}".FormatWith(this));

                // if an entity moves into the hex
                Assert.IsNull(m_content,
                                "m_content", "Hex {0} already has entity {1} and can't accept entity {2}"
                                .FormatWith(Coordinates, m_content, value));

                m_content = value;

                var otherHex = m_content.Hex;
                m_content.Hex = this;
                m_content.Mark(Position);

                if (otherHex != null)
                {
                    otherHex.Content = null;
                }

                var active = value as ActiveEntity;
                if (active != null)
                {
                    active.SetSeenHexes();
                }

                TacticalState.ResetAllActions();
            }
        }

        // the amount of entities that have seen this hex.
        // if it is positive then the hex should be clear. otherwise it should contain fog of war and,
        // if the entity in the hex is detected by radar, radar blips.
        private int SeenAmount
        {
            get { return m_seen; }
            set
            {
                m_seen = value;
                if (m_seen == 0)
                {
                    DisplayFogOfWarMarker();
                    if (DetectedAmount > 0)
                    {
                        DisplayRadarBlipMarker();
                    }
                    else
                    {
                        RemoveRadarBlipMarker();
                    }
                }
                else
                {
                    RemoveFogOfWarMarker();
                }
            }
        }

        // the amount of entities that have detected the entity in this hex
        private int DetectedAmount
        {
            get { return m_detected; }
            set
            {
                m_detected = value;
                if (m_detected == 0)
                {
                    RemoveRadarBlipMarker();
                }
                if (m_detected > 0 && m_seen == 0)
                {
                    DisplayRadarBlipMarker();
                }
            }
        }

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

        public void Init(Vector2 coordinates)
        {
            Coordinates = coordinates;
            s_repository.Add(coordinates, this);
            gameObject.name = ToString();
        }

        // constructor
        public HexReactor()
        {
            base.ClickableAction = CheckIfClickIsOnUI(() => TacticalState.SelectedHex = this);
            m_setMouseOverAction = () => { };
        }

        // return all hexes neighbouring current hex
        public IEnumerable<HexReactor> GetNeighbours()
        {
            var result = new List<HexReactor>();
            CheckAndAdd(result, new Vector2(Coordinates.x - 0.5f, Coordinates.y - 1));
            CheckAndAdd(result, new Vector2(Coordinates.x + 0.5f, Coordinates.y - 1));
            CheckAndAdd(result, new Vector2(Coordinates.x + 1.0f, Coordinates.y));
            CheckAndAdd(result, new Vector2(Coordinates.x - 1.0f, Coordinates.y));
            CheckAndAdd(result, new Vector2(Coordinates.x - 0.5f, Coordinates.y + 1));
            CheckAndAdd(result, new Vector2(Coordinates.x + 0.5f, Coordinates.y + 1));
            return result;
        }

        public int Distance(HexReactor other)
        {
            var yDist = Math.Abs(this.Coordinates.y - other.Coordinates.y);
            var xDist = Math.Abs(this.Coordinates.x - other.Coordinates.x);
            var correctedXDist = Math.Max(xDist - yDist / 2, 0);
            return (Int32)(correctedXDist + yDist);
        }

        public IEnumerable<HexReactor> RaycastAndResolve(int minRange, int maxRange, HexCheck addToListCheck, bool rayCastAll, string layerName)
        {
            return RaycastAndResolve<EntityReactor>(minRange, maxRange, addToListCheck, rayCastAll, (hex) => false, layerName, (ent) => ent.Hex);
        }

        // ray cast in a certain direction, and over a certain layer.
        // raycasting is sending a ray until it reaches a collider.
        // if rayCastAll is set, then the ray won't stop on the first collider it meets.
        public IEnumerable<HexReactor> RaycastAndResolve<T>(int minRange, int maxRange, HexCheck addToListCheck,
            bool rayCastAll, HexCheck breakCheck, string layerName, Func<T, HexReactor> hexExtractor) where T : MonoBehaviour
        {
            Assert.NotNull(Content, "Operating out of empty hex {0}".FormatWith(this));

            Content.collider2D.enabled = false;
            var results = new HashSet<HexReactor>();
            var layerMask = 1 << LayerMask.NameToLayer(layerName);
            var amountOfHexesToCheck = 6 * maxRange;
            var angleSlice = 360f / amountOfHexesToCheck;
            var rayDistance = renderer.bounds.size.x * maxRange;

            for (float currentAngle = 0f; currentAngle < 360f; currentAngle += angleSlice)
            {
                if (rayCastAll)
                {
                    // return all colliders that the ray passes through
                    var rayHits = Physics2D.RaycastAll(Position, new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)), rayDistance, layerMask);
                    foreach (var rayHit in rayHits)
                    {
                        var hex = hexExtractor(rayHit.collider.gameObject.GetComponent<T>());
                        if (Distance(hex) <= maxRange &&
                           Distance(hex) >= minRange &&
                           addToListCheck(hex))
                        {
                            results.Add(hex);
                        }
                        if (breakCheck(hex))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // return the first active collider in the ray's way
                    var rayHit = Physics2D.Raycast(Position, new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)), rayDistance, layerMask);
                    if (rayHit.collider != null)
                    {
                        var hex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Hex;
                        if (Distance(hex) <= maxRange &&
                           Distance(hex) >= minRange &&
                           addToListCheck(hex))
                        {
                            results.Add(hex);
                        }
                    }
                }
            }

            Content.collider2D.enabled = true;
            return results;
        }

        // checks if the target hex is in effect range from this hex. Used by the AI to evaluate far away hexes
        public bool CanAffect(HexReactor targetHex, DeliveryMethod deliveryMethod, int minRange, int maxRange, string layerName)
        {
            var distance = this.Distance(targetHex);
            if (deliveryMethod == DeliveryMethod.Unobstructed)
            {
                return (distance >= minRange && distance <= maxRange);
            }

            if (Content != null)
            {
                Content.collider2D.enabled = false;
            }
            var layerMask = 1 << LayerMask.NameToLayer(layerName);
            var angle = this.Position.GetAngleBetweenTwoPoints(targetHex.Position);
            var radianAngle = angle.DegreesToRadians();
            var directionVector = new Vector2(Mathf.Sin(radianAngle), Mathf.Cos(radianAngle));
            var rayHit = Physics2D.Raycast(Position, directionVector, renderer.bounds.size.x * distance, layerMask);

            Assert.NotNull(rayHit.collider, "ray collider");
            var hex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Hex;
            if (Content != null)
            {
                Content.collider2D.enabled = true;
            }
            return hex.Equals(targetHex);
        }

        public bool CanAffect(HexReactor targetHex, DeliveryMethod deliveryMethod, int minRange, int maxRange)
        {
            return CanAffect(targetHex, deliveryMethod, minRange, maxRange, "Entities");
        }

        #region sight

        public void Seen()
        {
            SeenAmount++;
        }

        public void Unseen()
        {
            SeenAmount--;
        }

        public void Detected()
        {
            DetectedAmount++;
        }

        public void Undetected()
        {
            DetectedAmount--;
        }

        public void ResetSight()
        {
            SeenAmount = 0;
            DetectedAmount = 0;
            RemoveTargetMarker();
        }

        #endregion sight

        #region object overrides

        public override string ToString()
        {
            return "Hex {0},{1}".FormatWith(Coordinates.x, Coordinates.y, Content, transform.position.x, transform.position.y);
        }

        public override int GetHashCode()
        {
            return Hasher.GetHashCode(Coordinates, Position);
        }

        public override bool Equals(object obj)
        {
            var hex = obj as HexReactor;
            return hex != null &&
                hex.Coordinates == Coordinates &&
                    hex.Position == Position;
        }

        #endregion object overrides

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
            if (Content != null)
            {
                Content.Mark();
            }
        }

        public void DisplayFogOfWarMarker()
        {
            m_fogOfWarMarker = AddAndDisplayMarker(m_fogOfWarMarker, "FogOfWar");
            if (Content != null)
            {
                Content.Unmark();
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

        // after an action is destroyed, check if there are actions still targetting this hex.
        public void RemoveTargetMarker(OperateSystemAction action)
        {
            if (TacticalState.SelectedHex == null)
            {
                RemoveMarker(m_targetMarker);
                return;
            }

            var Entity = TacticalState.SelectedHex.Content as EntityReactor;
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
            s_repository.Clear();
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
                    action.TargetedHex.RemoveTargetMarker();
                }
            }
        }

        public void AddCommands(EntityReactor Entity, List<OperateSystemAction> list)
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
                var Entity = TacticalState.SelectedHex.Content as EntityReactor;

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
                var Entity = TacticalState.SelectedHex.Content as EntityReactor;
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

        private void CheckAndAdd(IList<HexReactor> result, Vector2 coordinates)
        {
            HexReactor temp;
            if (s_repository.TryGetValue(coordinates, out temp))
            {
                result.Add(temp);
            }
        }

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
            var Entity = Content as ActiveEntity;
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
                var markerObject = ((GameObject)Instantiate(Resources.Load(markerName), Vector3.zero, Quaternion.identity));
                marker = markerObject.GetComponent<MarkerScript>();
                markerObject.gameObject.name = "{0} on {1}".FormatWith(markerName, this);
            }
            marker.Mark(transform.position);
            return marker;
        }

        #endregion private methods
    }
}