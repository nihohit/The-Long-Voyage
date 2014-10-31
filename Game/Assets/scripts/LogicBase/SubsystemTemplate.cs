using System;
using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    #region SubsystemTemplate

    /// <summary>
    /// Immutable templates for usable systems, and a static factory intializer.
    /// </summary>
    //TODO - how many operations per round does a system have? do we allow unlimited usage?
    public class SubsystemTemplate : IIdentifiable
    {
        #region properties

        public int MaxRange { get; private set; }

        public int MinRange { get; private set; }

        // How the system reaches hexes
        public DeliveryMethod DeliveryMethod { get; private set; }

        // The effect of the system
        public EntityEffectType Effect { get; private set; }

        // The stranegth of the system's effect
        public double EffectStrength { get; private set; }

        public string Name { get; private set; }

        // What entities can the system target
        public TargetingType PossibleTargets { get; private set; }

        public double EnergyCost { get; private set; }

        public double HeatGenerated { get; private set; }

        public int MaxAmmo { get; private set; }

        public HexEffectTemplate HexEffect { get; set; }

        #endregion properties

        #region constructor and initializer

        public SubsystemTemplate(int ammo,
                                double energyCost,
                                double heatGenerated,
                                int minRange,
                                int maxRange,
                                DeliveryMethod deliveryMethod,
                                string name,
                                EntityEffectType effectType,
                                double effectStrength,
                                TargetingType targetingType,
                                string hexEffectName)
        {
            MinRange = minRange;
            MaxRange = maxRange;
            Effect = effectType;
            EffectStrength = effectStrength;
            DeliveryMethod = deliveryMethod;
            Name = name;
            PossibleTargets = targetingType;
            EnergyCost = energyCost;
            MaxAmmo = ammo;
            HeatGenerated = heatGenerated;
            if (!String.IsNullOrEmpty(hexEffectName))
            {
                HexEffect = HexEffectTemplateStorage.Instance.GetConfiguration(hexEffectName);
            }
        }

        #endregion constructor and initializer
    }

    #endregion SubsystemTemplate

    #region SubsystemTemplateStorage

    public class SubsystemTemplateStorage : ConfigurationStorage<SubsystemTemplate, SubsystemTemplateStorage>
    {
        public SubsystemTemplateStorage()
            : base("Subsystems")
        { }

        protected override JSONParser<SubsystemTemplate> GetParser()
        {
            return new SubsystemTemplateParser();
        }

        #region SubsystemTemplateParser

        private class SubsystemTemplateParser : JSONParser<SubsystemTemplate>
        {
            protected override SubsystemTemplate ConvertCurrentItemToObject()
            {
                return new SubsystemTemplate(
                    TryGetValueOrSetDefaultValue<int>("Ammo", -1),
                    TryGetValueAndFail<float>("EnergyCost"),
                    TryGetValueAndFail<float>("HeatGenerated"),
                    TryGetValueOrSetDefaultValue<int>("MinRange", 0),
                    TryGetValueAndFail<int>("MaxRange"),
                    TryGetValueOrSetDefaultValue<DeliveryMethod>("DeliveryMethod", DeliveryMethod.Direct),
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<EntityEffectType>("EffectType"),
                    TryGetValueAndFail<float>("EffectStrength"),
                    TryGetValueOrSetDefaultValue<TargetingType>("TargetingType", TargetingType.Enemy),
                    TryGetValueOrSetDefaultValue<string>("HexEffect", null));
            }
        }

        #endregion SubsystemTemplateParser
    }

    #endregion SubsystemTemplateStorage
}