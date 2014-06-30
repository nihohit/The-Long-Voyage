using Assets.scripts.Base;
using Assets.scripts.LogicBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    public class Hex
    {
        #region private fields

        //holds all the hexes by their hex-coordinates
        private static Dictionary<Vector2, Hex> s_repository = new Dictionary<Vector2, Hex>();

        private TacticalEntity m_content = null;

        //these shouldn't be touched directly. There's a property for that.
        private int m_seen = 0, m_detected = 0;

        #endregion private fields

        #region properties

        public HexEffect Effects { get; private set; }

        public Biome BiomeType { get; private set; }

        public Vector2 Coordinates { get; private set; }

        public HexReactor Reactor { get; private set; }

        public Vector3 Position { get { return Reactor.transform.position; } }

        public TraversalConditions Conditions { get; set; }

        public TacticalEntity Content
        {
            get
            {
                return m_content;
            }
            set
            {
                if (TacticalState.BattleStarted)
                {
                    //using reference comparisons to account for null
                    if (value != m_content)
                    {
                        if (value != null)
                        {
                            Assert.IsNull(m_content,
                                          "m_content", "Hex {0} already has entity {1} and can't accept entity {2}"
                                            .FormatWith(Coordinates, m_content, value));
                            m_content = value;

                            var otherHex = m_content.Hex;
                            m_content.Hex = this;
                            m_content.Reactor.Mark(Position);

                            if (otherHex != null)
                            {
                                otherHex.Content = null;
                            }

                            var active = value as ActiveEntity;
                            if (active != null)
                            {
                                active.SetSeenHexes();
                            }
                        }
                        //if hex recieves null value as content
                        else
                        {
                            if (m_content != null)
                            {
                                Assert.AssertConditionMet((m_content.Destroyed()) ||
                                                          (m_content.Hex != null &&
                                                          !m_content.Hex.Equals(this)),
                                                          "When replaced with a null value, entity should either move to another hex or be destroyed");
                            }
                            m_content = null;
                        }
                        TacticalState.ResetAllActions();
                    }
                }
                //if the game hasn't started yet
                else
                {
                    m_content = value;
                    m_content.Hex = this;
                    m_content.Reactor.Mark(Position);
                }
            }
        }

        private int SeenAmount
        {
            get { return m_seen; }
            set
            {
                m_seen = value;
                if (m_seen == 0)
                {
                    Reactor.DisplayFogOfWarMarker();
                    if (DetectedAmount > 0)
                    {
                        Reactor.DisplayRadarBlipMarker();
                    }
                    else
                    {
                        Reactor.RemoveRadarBlipMarker();
                    }
                }
                else
                {
                    Reactor.RemoveFogOfWarMarker();
                }
            }
        }

        private int DetectedAmount
        {
            get { return m_detected; }
            set
            {
                m_detected = value;
                if (m_detected == 0)
                {
                    Reactor.RemoveRadarBlipMarker();
                }
                if (m_detected > 0 && m_seen == 0)
                {
                    Reactor.DisplayRadarBlipMarker();
                }
            }
        }

        #endregion properties

        #region constructor

        public Hex(Vector2 coordinates, HexReactor reactor)
        {
            Coordinates = coordinates;
            s_repository.Add(coordinates, this);
            Reactor = reactor;
        }

        public static void Init()
        {
            s_repository.Clear();
        }

        #endregion constructor

        #region public methods

        public IEnumerable<Hex> GetNeighbours()
        {
            var result = new List<Hex>();
            CheckAndAdd(result, new Vector2(Coordinates.x - 0.5f, Coordinates.y - 1));
            CheckAndAdd(result, new Vector2(Coordinates.x + 0.5f, Coordinates.y - 1));
            CheckAndAdd(result, new Vector2(Coordinates.x + 1.0f, Coordinates.y));
            CheckAndAdd(result, new Vector2(Coordinates.x - 1.0f, Coordinates.y));
            CheckAndAdd(result, new Vector2(Coordinates.x - 0.5f, Coordinates.y + 1));
            CheckAndAdd(result, new Vector2(Coordinates.x + 0.5f, Coordinates.y + 1));
            return result;
        }

        public int Distance(Hex other)
        {
            var yDist = Math.Abs(this.Coordinates.y - other.Coordinates.y);
            var xDist = Math.Abs(this.Coordinates.x - other.Coordinates.x);
            var correctedXDist = Math.Max(xDist - yDist / 2, 0);
            return (Int32)(correctedXDist + yDist);
        }

        public IEnumerable<Hex> RaycastAndResolve(int minRange, int maxRange, HexCheck addToListCheck, bool rayCastAll, string layerName)
        {
            return RaycastAndResolve<EntityReactor>(minRange, maxRange, addToListCheck, rayCastAll, (hex) => false, layerName, (ent) => ent.Entity.Hex);
        }

        public IEnumerable<Hex> RaycastAndResolve<T>(int minRange, int maxRange, HexCheck addToListCheck, bool rayCastAll, HexCheck breakCheck, string layerName, Func<T, Hex> hexExtractor) where T : MonoBehaviour
        {
            Assert.NotNull(Content, "Operating out of empty hex {0}".FormatWith(this));

            Content.Reactor.collider2D.enabled = false;
            var results = new HashSet<Hex>();
            var layerMask = 1 << LayerMask.NameToLayer(layerName);
            var amountOfHexesToCheck = 6 * maxRange;
            var angleSlice = 360f / amountOfHexesToCheck;
            var rayDistance = Reactor.renderer.bounds.size.x * maxRange;

            for (float currentAngle = 0f; currentAngle < 360f; currentAngle += angleSlice)
            {
                if (rayCastAll)
                {
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
                    var rayHit = Physics2D.Raycast(Position, new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)), rayDistance, layerMask);
                    if (rayHit.collider != null)
                    {
                        var hex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Entity.Hex;
                        if (Distance(hex) <= maxRange &&
                           Distance(hex) >= minRange &&
                           addToListCheck(hex))
                        {
                            results.Add(hex);
                        }
                    }
                }
            }

            Content.Reactor.collider2D.enabled = true;
            return results;
        }

        public bool CanAffect(Hex targetHex, DeliveryMethod deliveryMethod, int minRange, int maxRange, string layerName)
        {
            var distance = this.Distance(targetHex);
            if (deliveryMethod == DeliveryMethod.Unobstructed)
            {
                return (distance >= minRange && distance <= maxRange);
            }

            if (Content != null)
            {
                Content.Reactor.collider2D.enabled = false;
            }
            var layerMask = 1 << LayerMask.NameToLayer(layerName);
            var angle = this.Position.GetAngleBetweenTwoPoints(targetHex.Position);
            var radianAngle = angle.ToRadians();
            var directionVector = new Vector2(Mathf.Sin(radianAngle), Mathf.Cos(radianAngle));
            var rayHit = Physics2D.Raycast(Position, directionVector, Reactor.renderer.bounds.size.x * distance, layerMask);

            Assert.NotNull(rayHit.collider, "ray collider");
            var hex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Entity.Hex;
            if (Content != null)
            {
                Content.Reactor.collider2D.enabled = true;
            }
            return hex.Equals(targetHex);
        }

        public bool CanAffect(Hex targetHex, DeliveryMethod deliveryMethod, int minRange, int maxRange)
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
            Reactor.RemoveTargetMarker();
        }

        #endregion sight

        #region object overrides

        public override string ToString()
        {
            return "Hex {0},{1}".FormatWith(Coordinates.x, Coordinates.y, Content, Reactor.transform.position.x, Reactor.transform.position.y);
        }

        public override int GetHashCode()
        {
            return Hasher.GetHashCode(Coordinates, Position);
        }

        public override bool Equals(object obj)
        {
            var hex = obj as Hex;
            return hex != null &&
                hex.Coordinates == Coordinates &&
                    hex.Position == Position;
        }

        #endregion object overrides

        #endregion public methods

        #region private methods

        private void CheckAndAdd(IList<Hex> result, Vector2 coordinates)
        {
            Hex temp;
            if (s_repository.TryGetValue(coordinates, out temp))
            {
                result.Add(temp);
            }
        }

        #endregion private methods
    }
}