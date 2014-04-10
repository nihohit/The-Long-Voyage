using System;
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
                //if hex recieves null value as content
                else
                {
                    if(m_content != null)
                    {
                        Assert.AssertConditionMet((m_content.Health <= 0) || 
                                                  (m_content.Hex != null &&
                                                  !m_content.Hex.Equals(this)), 
                                                  "When replaced with a null value, entity should either move to another hex or be destroyed");
                    }
                    m_content = null;
                }
                TacticalState.ResetAllActions();
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
    //TODO - remove after testing
    private readonly string m_name;

    protected readonly ActiveEntity m_entity;

    public bool Destroyed { get ; private set;}

    #endregion

    #region properties

    public ActiveEntity ActingEntity { get; set; }

    #endregion

    #region constructor

    protected PotentialAction(ActiveEntity entity, string buttonName, Vector3 position)
    {
        Destroyed = false;
        m_button = ((GameObject)MonoBehaviour.Instantiate(Resources.Load(buttonName), position, Quaternion.identity)).GetComponent<CircularButton>();
        m_button.Action = Commit;
        m_button.Unmark();
        m_name = buttonName;
        m_entity = entity;
    } 

    #endregion

    #region public methods

    public virtual void DisplayButton()
    {
        //if the condition for this command still stands, display it. otherwise destroy it
        if(!Destroyed && NecessaryCondition())
        {
            m_button.Mark();
        }
        else
        {
            Destroy();
        }
    }

    public virtual void RemoveDisplay()
    {
        if (!Destroyed)
        {
            m_button.Unmark();
        }
    }

    public virtual void Destroy()
    {
        if (!Destroyed)
        {
            m_button.Unmark();
            UnityEngine.Object.Destroy(m_button.gameObject);
            Destroyed = true;
        }
    }

    public virtual void Commit()
    {
        Assert.AssertConditionMet(!Destroyed, "Action {0} was operated after being destroyed".FormatWith(this));
        Assert.EqualOrLesser(1, m_entity.Health, "{0} shouldn't be destroyed. Its condition is {1}".FormatWith(m_entity, m_entity.FullState()));
        AffectEntity();
        //makes it display all buttons;
        TacticalState.SelectedHex = TacticalState.SelectedHex;
        Debug.Log("{0} commited {1}".FormatWith(ActingEntity, m_name));
    }

    #endregion

    #region private methods

    //affects the acting entity with the action's costs
    protected abstract void AffectEntity();

    //represents the necessary conditions for the action to exist
    protected abstract bool NecessaryCondition();

    #endregion
}

public class MovementAction : PotentialAction
{
    #region private members

    private readonly IEnumerable<Hex> m_path;
    //TODO - does walking consume only movement points, or also energy (and if we implement that, produce heat)?
    private readonly double m_cost;

    #endregion

    #region constructors

    public MovementAction(MovingEntity entity, IEnumerable<Hex> path, double cost) : 
        base(entity, "movementMarker", path.Last().Position)
    {
        m_path = path;
        m_button.OnMouseOverProperty = DisplayPath;
        m_button.OnMouseExitProperty = RemovePath;
        m_cost = cost;
    }

    public MovementAction(MovingEntity entity, MovementAction action, Hex hex, double cost) : 
        this(entity, action.m_path.Union(new[]{hex}), cost)
    {
    }

    #endregion

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
            base.Commit();
            var lastHex = m_path.Last();
            lastHex.Content = ActingEntity;
            TacticalState.SelectedHex = null;
            //TODO - affects on commiting entity? Energy / heat cost, etc.?
            Destroy();
        }
    }

    protected override void AffectEntity()
    {
        var movingEntity = m_entity as MovingEntity;
        Assert.NotNull(movingEntity, "{0} should be a Moving Entity".FormatWith(m_entity));
        Assert.EqualOrLesser(m_cost, movingEntity.AvailableSteps, "{0} should have enough movement steps available. Its condition is {1}".FormatWith(m_entity, m_entity.FullState()));
        movingEntity.AvailableSteps -= m_cost;
    }

    protected override bool NecessaryCondition()
    {
        var movingEntity = m_entity as MovingEntity;
        Assert.NotNull(movingEntity, "{0} should be a Moving Entity".FormatWith(m_entity));
        return m_cost <= movingEntity.AvailableSteps;
    }

    #endregion
}

public class OperateSystemAction : PotentialAction
{
    private readonly Action m_action;
    private readonly double m_cost;

    public OperateSystemAction(ActiveEntity entity, HexOperation effect, string buttonName, Hex hex, Vector2 offset, double cost) : 
        base(entity, buttonName, (Vector2)hex.Position + (Vector2)offset)
    {
        m_action = ()=> effect(hex);
        m_cost = cost;
    }

    public override void Commit()
    {
        m_action();
        base.Commit();
    }

    protected override void AffectEntity()
    {
        Assert.EqualOrLesser(m_cost, m_entity.CurrentEnergy, "{0} should have enough energy available. Its condition is {1}".FormatWith(m_entity, m_entity.FullState()));
        m_entity.CurrentEnergy -= m_cost;
    }

    protected override bool NecessaryCondition()
    {
        return m_cost <= m_entity.CurrentEnergy;
    }
}

#endregion