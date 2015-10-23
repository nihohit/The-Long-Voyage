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

		// holds all the hexes by their hex-coordinates
		private static readonly Dictionary<Vector2, HexReactor> sr_repository = new Dictionary<Vector2, HexReactor>();

		// Only a single hex reactor can be selected at any time
		private static MarkerScript s_selected;
		private static PotentialActionsMarker s_actionsMarker;

		// markers
		private IUnityMarker m_movementPathMarker;
		private IUnityMarker m_fogOfWarMarker;
		private IUnityMarker m_radarBlipMarker;
		private IUnityMarker m_targetMarker;

		private EntityReactor m_content;

		// these shouldn't be touched directly. There's a property for that.
		private int m_seen, m_detected;

		#endregion private fields

		#region properties

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
				// if the game hasn't started yet
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
					Assert.AssertConditionMet(
						m_content.Destroyed() ||
						(m_content.Hex != null &&
						!m_content.Hex.Equals(this)),
						"When replaced with a null value, entity should either move to another hex or be destroyed");

					m_content = null;
					TacticalState.ResetAllActions();
					return;
				}

				// Debug.Log("Enter {0} to {1}".FormatWith(value, this));
				Assert.NotEqual(value, m_content, "Entered the same entity to hex {0}".FormatWith(this));

				// if an entity moves into the hex
				Assert.IsNull(
					m_content,
					"m_content",
					"Hex {0} already has entity {1} and can't accept entity {2}".FormatWith(Coordinates, m_content, value));

				m_content = value;

				var otherHex = m_content.Hex;
				m_content.Hex = this;

				if (otherHex != null)
				{
					otherHex.Content = null;
				}

				var activeEntity = value as ActiveEntity;
				if (activeEntity != null)
				{
					activeEntity.SetSeenHexes();
				}

				if (SeenAmount > 0)
				{
					m_content.Mark();
				}
			}
		}

		// the amount of entities that have seen this hex.
		// if it is positive then the hex should be clear. otherwise it should contain fog of war and,
		// if the entity in the hex is detected by radar, radar blips.
		private int SeenAmount
		{
			get
			{
				return m_seen;
			}

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
			get
			{
				return m_detected;
			}

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

		#endregion properties

		#region public methods

		public void Init(Vector2 coordinates)
		{
			Coordinates = coordinates;
			sr_repository.Add(coordinates, this);
			gameObject.name = ToString();
		}

		// constructor
		public HexReactor()
		{
			ClickableAction = CheckIfClickIsOnUI(() => TacticalState.SelectedHex = this);
			OnMouseOverAction = MouseOver;
		}

		private void MouseOver()
		{
			if(TacticalState.SelectedActiveEntity != null)
			{
				List<PotentialAction> actions;
				if(!TacticalState.SelectedActiveEntity.ActionsPerHex.TryGetValue(this, out actions) || actions == null || actions.None())
				{
					s_actionsMarker.gameObject.SetActive(false);
					return;
				}

				s_actionsMarker.SetOnHex(this, actions);
			}
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
			var yDifference = Math.Abs(Coordinates.y - other.Coordinates.y);
			var xDifference = Math.Abs(Coordinates.x - other.Coordinates.x);
			var complexDifference = Math.Abs(yDifference /2 + xDifference);
			return Convert.ToInt32(
				new[] {xDifference, yDifference, complexDifference }.Max());
		}

		// ray cast in a certain direction, and over a certain layer.
		// raycasting is sending a ray until it reaches a collider.
		// if rayCastAll is set, then the ray won't stop on the first collider it meets.
		public IEnumerable<HexReactor> RaycastAndResolveHexes(
			int minRange, 
			int maxRange, 
			HexCheck addToListCheck,
			HexCheck breakCheck,
			Color color)
		{
			Assert.NotNull(Content, "Operating out of empty hex {0}".FormatWith(this));
			var results = new HashSet<HexReactor>();
			var layerMask = 1 << this.gameObject.layer;
			var amountOfHexesToCheck = 6 * maxRange;
			var angleSlice = 360f / amountOfHexesToCheck;
			var rayDistance = GetComponent<Renderer>().bounds.size.x * maxRange;

			for (float currentAngle = 0f; currentAngle < 360f; currentAngle += angleSlice)
			{
				var angleInRadians = currentAngle.DegreesToRadians();
				// return all colliders that the ray passes through
				var rayHits = Physics2D.RaycastAll(Position, new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)), rayDistance, layerMask);
				Debug.DrawRay(Position, new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * rayDistance, color, 30);
				foreach (var hex in
					rayHits.Select(rayHit => rayHit.collider.gameObject.GetComponent<HexReactor>()))
				{
					if (RangeAndConditionalCheck(minRange, maxRange, addToListCheck, hex))
					{
						results.Add(hex);
					}

					if (breakCheck(hex))
					{
						break;
					}
				}
			}

			return results;
		}

		// checks if the target hex is in effect range from this hex. Used by the AI to evaluate far away hexes
		public bool CanAffect(HexReactor targetHex, DeliveryMethod deliveryMethod, int minRange, int maxRange)
		{
			var distance = Distance(targetHex);

			if(distance < minRange && distance > maxRange)
			{
				return false;
			}
			if (deliveryMethod == DeliveryMethod.Unobstructed) // we know it's in range from the last check
			{
				return true;
			}

			var layerMask = 1 << this.gameObject.layer;
			var angle = Position.GetAngleBetweenTwoPoints(targetHex.Position);
			var radianAngle = angle.DegreesToRadians();
			var directionVector = new Vector2(Mathf.Sin(radianAngle), Mathf.Cos(radianAngle));
			var rayHits = Physics2D.RaycastAll(Position, directionVector, GetComponent<Renderer>().bounds.size.x * distance, layerMask).ToList();

			Assert.NotNullOrEmpty(rayHits, "ray collider");
			var endHex = rayHits.FirstOrDefault(hex => hex.collider.GetComponent<HexReactor>().Content != null).collider.gameObject.GetComponent<HexReactor>();

			return targetHex.Equals(endHex);
		}

		#region sight

		public void Seen()
		{
			SeenAmount++;
		}

		public void LostSight()
		{
			SeenAmount--;
		}

		public void Detected()
		{
			DetectedAmount++;
		}

		public void LostDetection()
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

		public void DisplayTargetMarker()
		{
			m_targetMarker = AddAndDisplayMarker(m_targetMarker, "targetMarker");
		}

		#endregion markers

		public static void Init()
		{
			sr_repository.Clear();
			s_selected = GameObject.Find("Marker").GetComponent<MarkerScript>();
			s_selected.Unmark();
			s_actionsMarker = GameObject.Find("PotentialActionsMarker").GetComponent<PotentialActionsMarker>();
			s_actionsMarker.gameObject.SetActive(false);
		}

		// Select this hex
		public void Select()
		{
			//Debug.Log( "Highlighting hex {0}".FormatWith(MarkedHex));
			s_selected.Mark(transform.position);
		}

		// Unselect the hex, and remove all action markers
		public void Unselect()
		{
			//Debug.Log("Deselecting hex {0}".FormatWith(MarkedHex));
			s_selected.Unmark();
			s_actionsMarker.gameObject.SetActive(false);
		}

		#endregion public methods

		#region private methods

		private static void CheckAndAdd(IList<HexReactor> result, Vector2 coordinates)
		{
			HexReactor temp;
			if (sr_repository.TryGetValue(coordinates, out temp))
			{
				result.Add(temp);
			}
		}

		private bool RangeAndConditionalCheck(int minRange, int maxRange, HexCheck addToListCheck, HexReactor hex)
		{
			var distance = this.Distance(hex);
			return distance <= maxRange && distance >= minRange && addToListCheck(hex);
		}

		private void Awake()
		{
			DisplayFogOfWarMarker();
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
				var markerTemp = UnityHelper.Instantiate<MarkerScript>(markerName);
				markerTemp.gameObject.name = markerName;
				markerTemp.transform.SetParent(this.transform);
				marker = markerTemp;
			}

			marker.Mark(transform.position);
			return marker;
		}

		#endregion private methods
	}
}