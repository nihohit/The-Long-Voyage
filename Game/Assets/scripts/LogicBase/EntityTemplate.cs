using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    using Assets.Scripts.Base.JsonParsing;
    using Assets.Scripts.InterSceneCommunication;

    #region EntityTemplate

    /// <summary>
    /// Immutable representation of an entity of a certain type, and a static factory constructor.
    /// TODO - if we'll want entities with fixed systems, we'll need to add their templates here and merge them in the entity constructor
    /// </summary>
    public class EntityTemplate : IIdentifiable<string>
    {
        #region properties

        public double Health { get; private set; }

        public VisualProperties Visuals { get; private set; }

        public string Name { get; private set; }

        public double Armor { get; private set; }

        public int RadarRange { get; private set; }

        public int SightRange { get; private set; }

        public double MaxEnergy { get; private set; }

        public double MaxHeat { get; private set; }

        public double MaxShields { get; private set; }

        public double HeatLossRate { get; private set; }

        public double ShieldRechargeRate { get; private set; }

        public MovementType MovementMethod { get; private set; }

        public float MaxSpeed { get; private set; }

        public int SystemSlots { get; private set; }

        #endregion properties

        #region constructors

        public EntityTemplate(
            string name,
            int health,
            double maxEnergy,
            double maxHeat,
            double maxShields,
            float maximumSpeed,
            double heatLossRate = 1,
            double shieldRechargeRate = 1,
            int systemSlots = 3,
            MovementType movementType = MovementType.Walker,
            VisualProperties visualProperties = VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight,
            double armor = 0,
            int radarRange = 20,
            int sightRange = 10)
        {
            Name = name;
            Health = health;
            Visuals = visualProperties;
            Armor = armor;
            RadarRange = radarRange;
            SightRange = sightRange;
            MaxEnergy = maxEnergy;
            MaxHeat = maxHeat;
            MaxShields = maxShields;
            HeatLossRate = heatLossRate;
            ShieldRechargeRate = shieldRechargeRate;
            MovementMethod = movementType;
            MaxSpeed = maximumSpeed;
            SystemSlots = systemSlots;
        }

        #endregion constructors
    }

    #endregion EntityTemplate

    #region TerrainEntityTemplate

    public class TerrainEntityTemplate : EntityTemplate
    {
        public TerrainEntityTemplate(
            string name,
            int health,
            VisualProperties visualProperties = VisualProperties.AppearsOnSight) :
            base(name, health, 0, 0, 0, 0, 0, 0, 0, MovementType.Unmoving, visualProperties, 0, 0, 0)
        {
        }
    }

    #endregion TerrainEntityTemplate

    #region SpecificEntity

    /// <summary>
    /// Defines a specific entity by its template a modifying vairant.
    /// </summary>
    public class SpecificEntity
    {
        public EntityTemplate Template { get; private set; }

        public EntityVariant Variant { get; private set; }

        [ChosenConstructorForParsing]
        public SpecificEntity(string template, EntityVariant variant = EntityVariant.Regular) :
            this(GlobalState.Instance.Configurations.ActiveEntities.GetConfiguration(template), variant)
        {
        }

        public SpecificEntity(EntityTemplate template, EntityVariant variant = EntityVariant.Regular)
        {
            Template = template;
            Variant = variant;
        }

		public override int GetHashCode()
		{
			return Hasher.GetHashCode(Template, Variant);
		}

		public override bool Equals(object obj)
		{
			var ent = obj as SpecificEntity;
			return ent != null &&
				ent.Variant.Equals(Variant) &&
				ent.Template.Equals(Template);
        }
	}

    #endregion SpecificEntity
}