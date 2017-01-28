using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    using Assets.Scripts.InterSceneCommunication;

    #region SubsystemTemplate

    /// <summary>
    /// Immutable templates for usable systems, and a static factory initializer.
    /// </summary>
    public class SubsystemTemplate : IIdentifiable<string>
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

        public int ActionsPerTurn { get; set; }

        #endregion properties

        #region constructor

        public SubsystemTemplate(
                                double energyCost,
                                double heatGenerated,
                                int maxRange,
                                string name,
                                EntityEffectType effectType,
                                double effectStrength,
                                int minRange = 0,
                                DeliveryMethod deliveryMethod = DeliveryMethod.Direct,
                                TargetingType targetingType = TargetingType.Enemy,
                                string hexEffect = "",
                                int ammo = -1,
                                int actionsPerTurn = 1)
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
            this.ActionsPerTurn = actionsPerTurn;
            if (!string.IsNullOrEmpty(hexEffect))
            {
                HexEffect = GlobalState.Instance.Configurations.HexEffects.GetConfiguration(hexEffect);
            }
        }

        #endregion constructor

        #region object overrides

        public override bool Equals(object obj)
        {
            var template = obj as SubsystemTemplate;
            return template != null &&
                this.Name.Equals(template.Name);
        }

        public override int GetHashCode()
        {
            return Hasher.GetHashCode(this.Name);
        }

        public override string ToString()
        {
            return "{0}_Template".FormatWith(Name);
        }

        #endregion object overrides
    }

    #endregion SubsystemTemplate
}