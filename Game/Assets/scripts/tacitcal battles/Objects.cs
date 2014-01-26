using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#region hex

public class Hex
{
	private static Dictionary<Vector2, Hex> s_repository = new Dictionary<Vector2, Hex>();

	public Entity HexContent { get; set; }
	public int Elevation { get; private set;}
	public HexEffect Effects { get; private set; }
	public Biome BiomeType { get; private set; }
	public Vector2 Coordinates { get; private set; }
	public HexReactor Reactor {get; private set;}

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
}

#endregion

#region enums

public enum Biome { Tundra, City, Grass, Desert, Swamp}

[Flags]
public enum HexEffect 
{ 
	None = 0, 
	Slowing = 1,
	Heating = 2,
	Chilling = 4, 
}

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
	public Entity(DecisionType decision, EntityType type, double health, double shield, VisualProperties visuals, Hex hex)
	{
		if(hex.HexContent != null)
		{
			throw new Exception("hex not empty");
		}
		Decision = decision;
		Type = type;
		Health = health;
		Shield = shield;
		Visuals = visuals;
		Hex = hex;
		hex.HexContent = this;
	}

	public DecisionType Decision { get; private set; }
	
	public EntityType Type { get; private set; }
	
	public double Health { get; private set; }
	
	public double Shield { get; private set; }
	
	public VisualProperties Visuals { get; private set; }
	
	public Hex Hex { get; private set; }
}

public abstract class ActiveEntity : Entity
{
	public ActiveEntity(int actionsAmount, double radarRange, double sightRange, IEnumerable<Subsystem> systems, DecisionType decision, EntityType type, double health, double shield, VisualProperties visuals, Hex hex) : 
	base(decision, type, health, shield, visuals, hex)
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
	public MovingEntity(double speed, double radarRange, double sightRange, IEnumerable<Subsystem> systems, DecisionType decision, EntityType type, double health, double shield, VisualProperties visuals, Hex hex) : 
		base(2, radarRange, sightRange, systems, decision, type, health, shield, visuals, hex)
	{
		Speed = speed;
	}

	public double Speed { get; private set; }
}

public class Mech : MovingEntity
{
	public Mech(IEnumerable<Subsystem> systems, DecisionType decision, Hex hex, 
	            double health = 5, 
	            double shield = 3, 
	            VisualProperties visuals = VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight, 
	            double speed = 5, 
	            double radarRange = 20, 
	            double sightRange = 10) : 
		base(speed, radarRange, sightRange, systems, decision, EntityType.Mech, health, shield, visuals, hex)
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

