using Assets.Scripts.Base;

namespace Assets.Scripts.LogicBase
{
    public class HexEffectTemplate : IIdentifiable<string>
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
}