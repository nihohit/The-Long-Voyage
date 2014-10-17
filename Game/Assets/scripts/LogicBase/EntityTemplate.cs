using Assets.Scripts.Base;
using System;

namespace Assets.Scripts.LogicBase
{
    #region EntityTemplate

    /// <summary>
    /// Immutable representation of an entity of a certain type, and a static factory constructor.
    /// </summary>
    //TODO - we can create different levels (regular/active/moving) of templates for the different entities. Not sure it's needed now
    //TODO - if we'll want entities with fixed systems, we'll need to add their templates here and merge them in the entity constructor
    public class EntityTemplate : IIdentifiable
    {
        #region properties

        public double Health { get; private set; }

        public VisualProperties Visuals { get; private set; }

        public String Name { get; private set; }

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

        //for inanimate entities
        public EntityTemplate(string name, int health, VisualProperties visualProperties) :
            this(name, health, visualProperties, 0, 0, 0, 0, 0, 0, 0, 0, 0)
        { }

        //for unmoving entities
        public EntityTemplate(string name, int health, VisualProperties visualProperties, double armor,
            int radarRange, int sightRange, double maxEnergy, double maxHeat, double maxShields,
            double heatLossRate, double shieldRechargeRate, int systemSlots) :
            this(name, health, visualProperties, armor, radarRange, sightRange, maxEnergy,
            maxHeat, maxShields, heatLossRate, shieldRechargeRate, systemSlots, MovementType.Unmoving, 0)
        { }

        public EntityTemplate(string name, int health, VisualProperties visualProperties, double armor,
            int radarRange, int sightRange, double maxEnergy, double maxHeat, double maxShields,
            double heatLossRate, double shieldRechargeRate, int systemSlots, MovementType movementType, float maximumSpeed)
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

    #region SpecificEntity

    /// <summary>
    /// Defines a specific entity by its template a modifying vairant.
    /// </summary>
    public class SpecificEntity
    {
        public EntityTemplate Template { get; private set; }

        public EntityVariant Variant { get; private set; }

        public SpecificEntity(EntityTemplate template, EntityVariant variant)
        {
            Template = template;
            Variant = variant;
        }

        public SpecificEntity(EntityTemplate template)
            : this(template, EntityVariant.Regular)
        { }
    }

    #endregion SpecificEntity

    #region EntityTemplateStorage

    public class EntityTemplateStorage : ConfigurationStorage<EntityTemplate, EntityTemplateStorage>
    {
        public EntityTemplateStorage()
            : base("MovingEntities")
        { }

        protected override JSONParser<EntityTemplate> GetParser()
        {
            return new EntityTemplateParser();
        }

        #region EntityTemplateParser

        private class EntityTemplateParser : JSONParser<EntityTemplate>
        {
            protected override EntityTemplate ConvertCurrentItemToObject()
            {
                return new EntityTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<int>("Health"),
                    TryGetValueOrSetDefaultValue<VisualProperties>("VisualProperties",
                    VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight),
                    TryGetValueOrSetDefaultValue<float>("Armor", 0),
                    TryGetValueOrSetDefaultValue<int>("RadarRange", 20),
                    TryGetValueOrSetDefaultValue<int>("SightRange", 10),
                    TryGetValueAndFail<float>("MaxEnergy"),
                    TryGetValueAndFail<float>("MaxHeat"),
                    TryGetValueAndFail<float>("MaxShields"),
                    TryGetValueAndFail<float>("MaxHeatLoss"),
                    TryGetValueAndFail<float>("ShieldRechargeRate"),
                    TryGetValueOrSetDefaultValue<int>("SystemSlots", 3),
                    TryGetValueOrSetDefaultValue<MovementType>("MovementType", MovementType.Walker),
                    TryGetValueAndFail<float>("MaximumSpeed"));
            }
        }

        #endregion EntityTemplateParser
    }

    #endregion EntityTemplateStorage

    #region TerrainEntityTemplateStorage

    public class TerrainEntityTemplateStorage : ConfigurationStorage<EntityTemplate, TerrainEntityTemplateStorage>
    {
        public TerrainEntityTemplateStorage()
            : base("TerrainEntities")
        { }

        protected override JSONParser<EntityTemplate> GetParser()
        {
            return new TerrainEntityTemplateParser();
        }

        #region TerrainEntityTemplateParser

        private class TerrainEntityTemplateParser : JSONParser<EntityTemplate>
        {
            protected override EntityTemplate ConvertCurrentItemToObject()
            {
                return new EntityTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<int>("Health"),
                    (VisualProperties)TryGetValueOrSetDefaultValue<int>("VisualProperties", 2));
            }
        }

        #endregion TerrainEntityTemplateParser
    }

    #endregion TerrainEntityTemplateStorage
}