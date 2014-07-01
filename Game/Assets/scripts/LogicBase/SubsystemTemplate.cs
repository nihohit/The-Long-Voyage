using System;
using System.Collections.Generic;

namespace Assets.scripts.LogicBase
{
    #region SubsystemTemplate

    //TODO - how many operations per round does a system have? do we allow unlimited usage?
    public class SubsystemTemplate
    {
        #region fields

        private static readonly Dictionary<Int32, SubsystemTemplate> s_knownTemplates = new Dictionary<Int32, SubsystemTemplate>();

        #endregion fields

        #region properties

        public int MaxRange { get; private set; }

        public int MinRange { get; private set; }

        public DeliveryMethod DeliveryMethod { get; private set; }

        public EffectType Effect { get; private set; }

        public double EffectStrength { get; private set; }

        public string Name { get; private set; }

        public TargetingType PossibleTargets { get; private set; }

        public double EnergyCost { get; private set; }

        public double HeatGenerated { get; private set; }

        public int MaxAmmo { get; private set; }

        #endregion properties

        #region constructor and initializer

        private SubsystemTemplate(double energyCost,
                                  double heatGenerated,
                                  int minRange,
                                  int maxRange,
                                  DeliveryMethod deliveryMethod,
                                  string name,
                                  EffectType effectType,
                                  double effectStrength,
                                  TargetingType targetingType)
            : this(0, energyCost, heatGenerated, minRange, maxRange, deliveryMethod, name, effectType, effectStrength, targetingType)
        { }

        private SubsystemTemplate(int ammo,
                                double energyCost,
                                double heatGenerated,
                                int minRange,
                                int maxRange,
                                DeliveryMethod deliveryMethod,
                                string name,
                                EffectType effectType,
                                double effectStrength,
                                TargetingType targetingType)
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
        }

        public static SubsystemTemplate GetTemplate(Int32 id)
        {
            //TODO - no error handling at the moment.
            return s_knownTemplates[id];
        }

        //TODO - this method should be removed after we have initialization from XML
        public static void Init()
        {
            if (s_knownTemplates.Count == 0)
            {
                s_knownTemplates.Add(0,
                                     new SubsystemTemplate(1, 0, 0, 2, DeliveryMethod.Direct, "Emp", EffectType.EmpDamage, 1.5f, TargetingType.Enemy));
                s_knownTemplates.Add(1,
                                     new SubsystemTemplate(2, 2, 0, 4, DeliveryMethod.Direct, "Laser", EffectType.PhysicalDamage, 2f, TargetingType.Enemy));
                s_knownTemplates.Add(2,
                                     new SubsystemTemplate(4, 1, 0, 2, 6, DeliveryMethod.Unobstructed, "Missile", EffectType.PhysicalDamage, 1f, TargetingType.Enemy));
                s_knownTemplates.Add(3,
                                     new SubsystemTemplate(2, 2, 2, 0, 2, DeliveryMethod.Unobstructed, "Flamer", EffectType.FlameHex, 1f, TargetingType.AllHexes));
                s_knownTemplates.Add(4,
                                     new SubsystemTemplate(2, 1, 0, 3, DeliveryMethod.Direct, "HeatWave", EffectType.HeatDamage, 2f, TargetingType.Enemy));
                s_knownTemplates.Add(5,
                                     new SubsystemTemplate(10, 0.5, 0.5, 0, 4, DeliveryMethod.Direct, "IncediaryGun", EffectType.IncendiaryDamage, 1.5f, TargetingType.Enemy));
            }
        }

        #endregion constructor and initializer
    }

    #endregion
}