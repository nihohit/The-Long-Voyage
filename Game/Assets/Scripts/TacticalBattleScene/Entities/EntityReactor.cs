using System;
using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
	#region EntityReactor

	/// <summary>
	/// A script wrapper for entities
	/// </summary>
	public abstract class EntityReactor : MarkerScript
	{
		#region private fields

		private static int s_idCounter;

		private readonly int m_id;

		#endregion private fields

		#region properties

		public int ID { get { return m_id; } }

		public double Health { get; private set; }

		public virtual HexReactor Hex { get; set; }

		public Loyalty Loyalty { get; private set; }

		public String Name { get; private set; }

		public EntityTemplate Template { get; private set; }

		#endregion properties

		#region constructor

		protected EntityReactor()
		{
			m_id = s_idCounter++;
		}

		#endregion constructor

		#region public methods

		// Change the entity's state. Usually called when a subsystem operates on the entity.
		// TODO - currently only damages the unit
		public void Affect(double strength, EntityEffectType effectType)
		{
			Debug.Log("{0} was hit for damage {1} and type {2}".FormatWith(Name, strength, effectType));
			Debug.Log(FullState());
			var remainingDamage = ExternalDamage(strength, effectType);
			InternalDamage(remainingDamage, effectType);
			Debug.Log(FullState());

			if (Destroyed())
			{
				Destroy();
			}
		}

		// this function returns a string value that represents the mutable state of the entity
		public virtual string FullState()
		{
			return "{0}: Health {1}/{2} Hex {3}".FormatWith(Name, Health, Template.Health, Hex);
		}

		// just a simple function to make the code more readable
		public virtual bool Destroyed()
		{
			return Health <= 0;
		}

		#region object overrides

		public override bool Equals(object obj)
		{
			var ent = obj as EntityReactor;
			return ent != null &&
				ID == ent.ID;
		}

		public override int GetHashCode()
		{
			return Hasher.GetHashCode(Name, m_id);
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion object overrides

		#endregion public methods

		#region protected methods

		// after damage passes through armor & shields, it reduces health
		protected virtual void InternalDamage(double damage, EntityEffectType damageType)
		{
			switch (damageType)
			{
				case EntityEffectType.PhysicalDamage:
					Health -= damage;
					break;

				case EntityEffectType.IncendiaryDamage:
					Health -= damage / 2;
					break;
			}
		}

		protected void Init(SpecificEntity entity, Loyalty loyalty)
		{
			Template = entity.Template;
			Loyalty = loyalty;
			Health = Template.Health;
			Name = "{0} {1} {2}".FormatWith(Template.Name, Loyalty, m_id);
			gameObject.name = Name;
			if ((Template.Visuals & VisualProperties.AppearsOnRadar) != 0)
			{
				TacticalState.AddRadarVisibleEntity(this);
			}
			TacticalState.TextureManager.UpdateEntityTexture(this);
		}

		// reduce damage by the armor level, wehn relevant.
		protected virtual double ExternalDamage(double strength, EntityEffectType damageType)
		{
			switch (damageType)
			{
				case EntityEffectType.PhysicalDamage:
				case EntityEffectType.IncendiaryDamage:
					//TODO - Can armor be ablated away? if so, it needs to be copied over into a local field in the entity
					return strength - Template.Armor;

				case EntityEffectType.EmpDamage:
				case EntityEffectType.HeatDamage:
					return strength;

				default:
					throw new UnknownValueException(damageType);
			}
		}

		// destroy the entity
		// TODO - log the reason it was destroyed
		protected virtual void Destroy()
		{
			Debug.Log("Destroy {0}".FormatWith(Name));
			Hex.Content = null;
			TacticalState.DestroyEntity(this);
			DestroyGameObject();
		}

		#endregion protected methods
	}

	#endregion EntityReactor
}