using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.LogicBase
{
    #region HexEffectTemplate

    public class HexEffectTemplate : IIdentifiable
    {
        #region properties

        public double Power { get; private set; }

        public string Name { get; private set; }

        public EntityEffectType EffectType { get; private set; }

        public int Duration { get; private set; }

        #endregion properties

        #region constructors

        public HexEffectTemplate(string name, EntityEffectType effectType, double power, int duration)
        {
            Name = name;
            EffectType = effectType;
            Power = power;
            Duration = duration;
        }

        #endregion constructors
    }

    #endregion HexEffectTemplate

    #region HexEffectTemplateStorage

    public class HexEffectTemplateStorage : ConfigurationStorage<HexEffectTemplate, HexEffectTemplateStorage>
    {
        public HexEffectTemplateStorage()
            : base("HexEffects")
        { }

        protected override JSONParser<HexEffectTemplate> GetParser()
        {
            return new HexEffectTemplateParser();
        }

        #region HexEffectTemplateParser

        private class HexEffectTemplateParser : JSONParser<HexEffectTemplate>
        {
            protected override HexEffectTemplate ConvertCurrentItemToObject()
            {
                return new HexEffectTemplate(
                    TryGetValueAndFail<string>("Name"),
                    TryGetValueAndFail<EntityEffectType>("EffectType"),
                    TryGetValueAndFail<float>("Power"),
                    TryGetValueAndFail<int>("Duration"));
            }
        }

        #endregion HexEffectTemplateParser
    }

    #endregion HexEffectTemplateStorage
}