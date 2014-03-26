using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Hex

public class Hex
{
    #region private fields

    //holds all the hexes by their hex-coordinates
	private static Dictionary<Vector2, Hex> s_repository = new Dictionary<Vector2, Hex>();

	private Entity m_content = null;

    private int m_seen = 0, m_detected = 0;


    #endregion
	
    #region properties

	public HexEffect Effects { get; private set; }
	public Biome BiomeType { get; private set; }
	public Vector2 Coordinates { get; private set; }
    public HexReactor Reactor { get; private set; }
    public Vector3 Position { get { return Reactor.transform.position; } }
	public TraversalConditions Conditions { get; set; }
	public Entity Content 
	{ 
		get
		{
			return m_content;
		}
		set
		{
            //using reference comparisons to account for null
            if(value != m_content)
            {
                TacticalState.ResetAllActions();
                if(value != null)
                {
                    Assert.IsNull(m_content, 
                                  "m_content", "Hex {0} already has entity {1} and can't accept entity {2}"
                                    .FormatWith(Coordinates, m_content, value));
                    m_content = value;

                    var otherHex = m_content.Hex;
                    m_content.Hex = this;
                    m_content.Marker.Mark(Position);

                    if(otherHex != null)
                    {
                        otherHex.Content = null;
                    }

                    var active = value as ActiveEntity;
                    if(active != null)
                    {
                        active.SetSeenHexes();
                    }
                }
                else
                {
                    if(m_content != null)
                    {
                        Assert.AssertConditionMet((m_content.Health <= 0) || 
                                                  (m_content.Hex != null &&
                                                  !m_content.Hex.Equals(this)), 
                                                  "When replaced with a null value, entity should either move to another hex or be destroyed");
                    }
                    m_content = value;
                }
            }
		}
	}

    #endregion

    #region constructor

	public Hex(Vector2 coordinates, HexReactor reactor)
	{
		Coordinates = coordinates;
		s_repository.Add(coordinates, this);
        Reactor = reactor;
	}

    #endregion

    #region public methods

	public IEnumerable<Hex> GetNeighbours()
	{
		var result = new List<Hex>();
		CheckAndAdd(result, new Vector2(Coordinates.x - 0.5f , Coordinates.y - 1));
		CheckAndAdd(result, new Vector2(Coordinates.x + 0.5f , Coordinates.y - 1));
		CheckAndAdd(result, new Vector2(Coordinates.x + 1.0f , Coordinates.y));
		CheckAndAdd(result, new Vector2(Coordinates.x - 1.0f , Coordinates.y));
		CheckAndAdd(result, new Vector2(Coordinates.x - 0.5f , Coordinates.y + 1));
		CheckAndAdd(result, new Vector2(Coordinates.x + 0.5f , Coordinates.y + 1));
		return result;
	}

    public int Distance(Hex other)
    {
        var yDist = Math.Abs(this.Coordinates.y - other.Coordinates.y);
        var xDist = Math.Abs(this.Coordinates.x - other.Coordinates.x);
        var correctedXDist = Math.Max(xDist - yDist/2, 0);
        return (Int32)(correctedXDist + yDist);
    }

    public IEnumerable<Hex> RaycastAndResolve(int minRange, int maxRange, HexCheck addToListCheck, bool rayCastAll, string layerName)
    {
        return RaycastAndResolve<EntityReactor>(minRange, maxRange, addToListCheck, rayCastAll, (hex) => false, layerName, (ent) => ent.Entity.Hex);
    }

    public IEnumerable<Hex> RaycastAndResolve<T>(int minRange, int maxRange, HexCheck addToListCheck, bool rayCastAll, HexCheck breakCheck, string layerName, Func<T, Hex> hexExtractor) where T : MonoBehaviour
    {
        Assert.NotNull(Content, "Operating out of empty hex {0}".FormatWith(this));
        
        Content.Marker.collider2D.enabled = false;
        var results = new HashSet<Hex>();
        var layerMask = 1 << LayerMask.NameToLayer(layerName);
        var amountOfHexesToCheck = 6*maxRange;
        var angleSlice = 360f / amountOfHexesToCheck;
        var rayDistance =  Reactor.renderer.bounds.size.x * maxRange;
        
        for(float currentAngle = 0f ; currentAngle < 360f ; currentAngle+= angleSlice)
        {
            if(rayCastAll)
            {
                var rayHits = Physics2D.RaycastAll(Position, new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)), rayDistance, layerMask);
                foreach(var rayHit in rayHits)
                {
                    var hex = hexExtractor(rayHit.collider.gameObject.GetComponent<T>());
                    if(Distance(hex) < maxRange && 
                       Distance(hex) >= minRange && 
                       addToListCheck(hex))
                    {
                        results.Add(hex);
                    }
                    if(breakCheck(hex))
                    {
                        break;
                    }
                }
            }
            else
            {
                var rayHit = Physics2D.Raycast(Position, new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)), rayDistance, layerMask);
                if(rayHit.collider != null)
                {
                    var hex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Entity.Hex;
                    if(Distance(hex) < maxRange && 
                       Distance(hex) >= minRange && 
                       addToListCheck(hex))
                    {
                        results.Add(hex);
                    }
                }
            }
        }
        
        Content.Marker.collider2D.enabled = true;
        return results;
    }

    #region sight

    public void Seen()
    {
        m_seen++;
        if(m_seen == 1)
        {
            Reactor.RemoveFogOfWarMarker();
        }
    }

    public void Unseen()
    {
        m_seen--;
        if(m_seen == 0)
        {
            Reactor.DisplayFogOfWarMarker();
        }
    }

    public void Detected()
    {
        m_detected++;
        if(m_detected == 1 && m_seen == 0)
        {
            Reactor.DisplayRadarBlipMarker();
        }
    }

    public void Undetected()
    {
        m_detected--;
        if(m_detected == 0)
        {
            Reactor.RemoveRadarBlipMarker();
        }
    }

    public void ResetSight()
    {
        m_seen = 0;
        m_detected = 0;
    }

    #endregion

    #region object overrides

    public override string ToString()
    {
        return "Hex {0},{1} : {2} : {3},{4}".FormatWith(Coordinates.x, Coordinates.y, Content, Reactor.transform.position.x, Reactor.transform.position.y);
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

    #endregion

    #endregion

    #region private methods

    private void CheckAndAdd(IList<Hex> result, Vector2 coordinates)
    {
        Hex temp;
        if(s_repository.TryGetValue(coordinates, out temp))
        {
            result.Add(temp);
        }
    }

    #endregion
}

#endregion

#region enums

public enum Biome { Tundra, City, Grass, Desert, Swamp}

[Flags]
public enum HexEffect 
{ 
	None = 0, 
	Heating = 1,
	Chilling = 2, 
}

//the logic behind the numbering is that the addition of this enumerator and MovementType gives the following result - if the value is between 0-5, no movement penalty. above 4 - slow, above 6 - impossible
public enum TraversalConditions { 
	Easy = 0, 
	Uneven = 1, //hard to crawl, everything else is fine
	Broken = 2, //hard to crawl or walk, everything else is fine
	NoLand = 4, //can't crawl or walk, everything else is fine
	Blocked = 5 //can only fly
}

// see TraversalAbility's comment
public enum MovementType { Crawler = 3, Walker = 2, Hover = 1, Flyer = 0 }


[Flags]
public enum VisualProperties
{
	None = 0,
	AppearsOnRadar = 1,
	AppearsOnSight = 2,
    BlocksSight = 4, 
}

public enum DamageType { EMP, Heat, Physical, Energy, }

public enum WeaponType { }

//to be filled with all different sides
public enum Loyalty { Player, EnemyArmy, Monsters, Bandits, Neutral }

// there needs to be an order of importance - the more severe damage has a higher value
public enum SystemCondition { Operational = 0, OutOfAmmo = 1, Neutralized = 2, Destroyed = 3 }

// the way a system reaches its targets
public enum DeliveryMethod { Direct, Unobstructed }

#endregion

#region delegates

public delegate bool EntityCheck (Entity ent);
public delegate void HexOperation (Hex hex);
public delegate double HexTraversalCost(Hex hex);
public delegate bool HexCheck(Hex hex);

#endregion

#region actions

/*
 * Potential action represents a certain action, commited by a certain Entity. 
 * When ordered to it can create a button that when pressed activates it, 
 * it can remove the button from the display and it should destroy the button when destroyed.
 * The button should receive the item's commit method as it's response when pressed.
 */
public abstract class PotentialAction
{
    #region fields

    protected readonly CircularButton m_button;
    private bool m_destroyed;
    //TODO - remove after testing
    private readonly string m_name;

    #endregion

    #region properties

    public ActiveEntity ActingEntity { get; set; }

    #endregion

    #region constructor

    protected PotentialAction(string buttonName, Vector3 position)
    {
        m_destroyed = false;
        m_button = ((GameObject)MonoBehaviour.Instantiate(Resources.Load(buttonName), position, Quaternion.identity)).GetComponent<CircularButton>();
        m_button.Action = Commit;
        m_button.Unmark();
        m_name = buttonName;
    } 

    #endregion

    #region public methods

    public virtual void DisplayButton()
    {
        m_button.Mark();
    }

    public virtual void RemoveDisplay()
    {
        m_button.Unmark();
    }

    public virtual void Destroy()
    {
        if (!m_destroyed)
        {
            m_button.Unmark();
            UnityEngine.Object.Destroy(m_button.gameObject);
            m_destroyed = false;
        }
    }

    public virtual void Commit()
    {
        Assert.AssertConditionMet(!m_destroyed, "Action {0} was operated after being destroyed".FormatWith(this));
        Debug.Log("{0} commited {1}".FormatWith(ActingEntity, m_name));
    }

    #endregion
}

public class MovementAction : PotentialAction
{
    #region constructors

    public MovementAction(IEnumerable<Hex> path) : 
        base("movementMarker", path.Last().Position)
    {
        m_path = path;
        m_button.OnMouseOverProperty = DisplayPath;
        m_button.OnMouseExitProperty = RemovePath;
    }

    public MovementAction(MovementAction action, Hex hex) : 
        this(action.m_path.Union(new[]{hex}))
    {
    }

    #endregion

    private readonly IEnumerable<Hex> m_path;

    #region private methods

    private void DisplayPath()
    {
        foreach (var hex in m_path)
        {
            hex.Reactor.DisplayMovementMarker();
        }
    }

    private void RemovePath()
    {
        foreach (var hex in m_path)
        {
            hex.Reactor.RemoveMovementMarker();
        }
    }

    #endregion

    #region overloaded methods

    public override void RemoveDisplay()
    {
        base.RemoveDisplay();
        RemovePath();
    }

    public override void Destroy()
    {
        RemovePath();
        base.Destroy();
    }

    public override void Commit()
    {
        lock (TacticalState.Lock)
        {
            var lastHex = m_path.Last();
            lastHex.Content = ActingEntity;
            TacticalState.SelectedHex = null;
            base.Commit();
            //TODO - affects on commiting entity? Energy / heat cost, etc.?
            Destroy();
        }
    }

    #endregion
}

public class OperateSystemAction : PotentialAction
{
    private readonly Action m_action;

    public OperateSystemAction(HexOperation effect, string buttonName, Hex hex, Vector2 offset) : 
        base(buttonName, (Vector2)hex.Position + (Vector2)offset)
    {
        m_action = ()=> effect(hex);
    }

    public override void Commit()
    {
        m_action();
        base.Commit();
    }
}

#endregion