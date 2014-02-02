using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#region hex

public class Hex
{
	private static Dictionary<Vector2, Hex> s_repository = new Dictionary<Vector2, Hex>();

	private Entity m_content = null;
	
	public HexEffect Effects { get; private set; }
	public Biome BiomeType { get; private set; }
	public Vector2 Coordinates { get; private set; }
	public HexReactor Reactor {get; private set;}
	public TraversalConditions Conditions { get; private set; }
	public Entity Content 
	{ 
		get
		{
			return m_content;
		}
		set
		{
            if(value == null)
            {
                m_content = value;
            }
            else
            {
                if(m_content != null)
                {
                    throw new Exception("Hex {0} already has entity {1} and can't accept entity {2}".FormatWith(Coordinates, m_content, value));
                }
                m_content = value;
                if(m_content.Hex != null)
                {
				    m_content.Hex.Content = null;
                }
				m_content.Hex = this;
                m_content.Marker.Mark(Reactor.transform.position);
            }
		}
	}

	public Hex(Vector2 coordinates, HexReactor reactor)
	{
		Coordinates = coordinates;
		s_repository.Add(coordinates, this);
		Reactor = reactor;
	}

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

	private void CheckAndAdd(List<Hex> result, Vector2 coordinates)
	{
		Hex temp;
		if(s_repository.TryGetValue(coordinates, out temp))
		{
			result.Add(temp);
		}
	}

	public float Dist(Hex other)
	{
		return Math.Max(
			Math.Abs(Coordinates.x - other.Coordinates.x),
			Math.Max(Math.Abs(Coordinates.y - other.Coordinates.y),
			Math.Abs(Coordinates.x + Coordinates.y - other.Coordinates.x- other.Coordinates.y)));
	}
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
}

public enum EntityType { Crawler, Mech, Monster, Infantry, Tank, Artilerry}

public enum DecisionType { Passive, PlayerControlled, AI }

public enum DamageType { EMP, Heat, Physical, Energy, }

public enum WeaponType { }

// there needs to be an order of importance - the more severe damage has a higher value
public enum Condition { Operational = 0, OutOfAmmo = 1, Neutralized = 2, Destroyed = 3}

#endregion

#region delegates

public delegate bool EntityCheck (Entity ent);
public delegate void HexOperation (Hex hex);
public delegate double HexTraversalCost(Hex hex);
public delegate bool HexCheck(Hex hex);

#endregion

#region Entities

public abstract class Entity
{

	public Entity(DecisionType decision, EntityType type, double health, double shield, VisualProperties visuals)
	{
		Decision = decision;
		Type = type;
		Health = health;
		Shield = shield;
		Visuals = visuals;
	}

	public MarkerScript Marker { get; set; }

	public DecisionType Decision { get; private set; }
	
	public EntityType Type { get; private set; }
	
	public double Health { get; private set; }
	
	public double Shield { get; private set; }
	
	public VisualProperties Visuals { get; private set; }
	
	public Hex Hex { get; set; }
}

public abstract class ActiveEntity : Entity
{
	public ActiveEntity(int actionsAmount, double radarRange, double sightRange, IEnumerable<Subsystem> systems, DecisionType decision, EntityType type, double health, double shield, VisualProperties visuals) : 
	base(decision, type, health, shield, visuals)
	{
		TotalActions = actionsAmount;
		RadarRange = radarRange;
		SightRange = sightRange;
		Systems = systems;
	}

	public int Actions { get; set; }
	
	public int TotalActions { get; private set; }
	
	public double RadarRange { get; private set; }
	
	public double SightRange { get; private set; }
	
	public IEnumerable<Subsystem> Systems { get; private set; }
}

public abstract class MovingEntity : ActiveEntity
{
	public MovingEntity(MovementType movement, double speed, double radarRange, double sightRange, IEnumerable<Subsystem> systems, DecisionType decision, EntityType type, double health, double shield, VisualProperties visuals) : 
		base(2, radarRange, sightRange, systems, decision, type, health, shield, visuals)
	{
		Speed = speed;
		TraversalMethod = movement;
	}

	public double Speed { get; private set; }

	public MovementType TraversalMethod {get; private set; }
}

public class Mech : MovingEntity
{
	public Mech(IEnumerable<Subsystem> systems, DecisionType decision,  
	            double health = 5, 
	            double shield = 3, 
	            VisualProperties visuals = VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight, 
	            double speed = 5, 
	            double radarRange = 20, 
	            double sightRange = 10) : 
		base(MovementType.Walker, speed, radarRange, sightRange, systems, decision, EntityType.Mech, health, shield, visuals)
	{	}
}


#endregion

#region subsystems

public abstract class Subsystem
{
	private Condition m_condition;
	
	public int MaxRange { get; private set; }
	public int MinRange { get; private set; }
	public abstract EntityCheck IsBlocked { get; }
	public abstract HexOperation Effect { get; }
	
	public Condition OperationalCondition
	{
		get
		{
			return m_condition;
		}
		
		private set
		{
			//condition most be ordered by order of severity
			if (value > m_condition)
			{
				m_condition = value;
			}
		}
	}
	
	public virtual void Hit(DamageType type)
	{
		switch (type)
		{
		case (DamageType.EMP):
		{
			OperationalCondition = Condition.Neutralized;
		}
			break;
		default:
			OperationalCondition = Condition.Destroyed;
			break;
		}
	}
	
	public bool Operational()
	{
		return m_condition == Condition.Operational;
	}
}

#region WeaponBase

public abstract class WeaponBase : Subsystem
{
	private static readonly Dictionary<WeaponType, WeaponBase> s_weaponFactory = new Dictionary<WeaponType, WeaponBase>();
	
	public bool DirectFire { get; private set; }
	public DamageType DamageType { get; private set; }
	public bool ShieldPiercing { get; private set; }
	public int Range { get; private set; }
	
	public WeaponBase GetInstance(WeaponType type)
	{
		WeaponBase result = null;
		if(!s_weaponFactory.TryGetValue(type, out result))
		{
			
		}
		return result;
	}
}

public abstract class AmmoWeapon : WeaponBase
{
	private int m_ammo;
}

#endregion

#endregion



