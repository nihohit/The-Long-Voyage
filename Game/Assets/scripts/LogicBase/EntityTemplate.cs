using Assets.scripts.Base;
using Assets.scripts.UnityBase;
using System;
using System.Collections.Generic;

namespace Assets.scripts.LogicBase
{
    #region EntityTemplate

    //TODO - we can create different levels of templates for the different entities. Not sure it's needed now
    public class EntityTemplate
    {
        #region fields

        private static readonly Dictionary<Int32, EntityTemplate> s_knownTemplates = new Dictionary<Int32, EntityTemplate>();

        #endregion fields

        #region properties

        //TODO - if we'll want entities with fixed systems, we'll need to add their templates here and merge them in the entity constructor
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

        public double MaxSpeed { get; private set; }

        public int SystemSlots { get; private set; }

        #endregion properties

        #region constructors

        //for inanimate entities
        private EntityTemplate(string name, int health, VisualProperties visualProperties) :
            this(name, health, visualProperties, 0, 0, 0, 0, 0, 0, 0, 0)
        { }

        //for unmoving entities
        private EntityTemplate(string name, int health, VisualProperties visualProperties, double armor,
            int radarRange, int sightRange, double maxEnergy, double maxHeat, double maxShields,
            double heatLossRate, double shieldRechargeRate) :
            this(name, health, visualProperties, armor, radarRange, sightRange, maxEnergy, maxHeat, maxShields, heatLossRate, shieldRechargeRate, MovementType.Unmoving, 0)
        { }

        private EntityTemplate(string name, int health, VisualProperties visualProperties, double armor,
            int radarRange, int sightRange, double maxEnergy, double maxHeat, double maxShields,
            double heatLossRate, double shieldRechargeRate, MovementType movementType, double maximumSpeed)
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
        }

        #endregion constructors

        #region static methods

        public static EntityTemplate GetTemplate(Int32 type)
        {
            //TODO - no error handling at the moment.
            return s_knownTemplates[type];
        }

        //TODO - this method should be removed after we have initialization from XML
        public static void Init()
        {
            if (s_knownTemplates.Count == 0)
            {
                s_knownTemplates.Add(1, new EntityTemplate("Mech", 5,
                    VisualProperties.AppearsOnRadar | VisualProperties.AppearsOnSight,
                    0, 20, 10, 2, 5, 3, 1, 1, MovementType.Walker, 4));
                s_knownTemplates.Add(2, new EntityTemplate("Dense trees",
                    FileHandler.GetIntProperty("Dense trees health", FileAccessor.Units), VisualProperties.AppearsOnSight | VisualProperties.BlocksSight));
                s_knownTemplates.Add(3, new EntityTemplate("Sparse trees",
                    FileHandler.GetIntProperty("Sparse trees health", FileAccessor.Units), VisualProperties.AppearsOnSight));
                s_knownTemplates.Add(4, new EntityTemplate("Building",
                    FileHandler.GetIntProperty("Building health", FileAccessor.Units), VisualProperties.AppearsOnSight | VisualProperties.BlocksSight | VisualProperties.AppearsOnRadar));
            }
        }

        #endregion static methods
    }

    #endregion

    #region SpecificEntity

    public class SpecificEntity
    {
        public EntityTemplate Template { get; private set; }
        public EntityVariant Variant { get; private set; }

        public SpecificEntity(EntityTemplate template, EntityVariant variant)
        {
            Template = template;
            Variant = variant;
        }

        public SpecificEntity(EntityTemplate template) : this(template, EntityVariant.Regular)
        { }
    }

    #endregion
}