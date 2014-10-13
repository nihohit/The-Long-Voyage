using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;

namespace Assets.Scripts.TacticalBattleScene
{
    #region inanimate entities

    /// <summary>
    /// Entities which represent natural pieces of terrain, and aren't active - just obstructions.
    /// </summary>
    public class TerrainEntity : EntityReactor
    {
        private HexReactor m_hex;

        public void Init(EntityTemplate template)
        {
            base.Init(new SpecificEntity(template), Loyalty.Inactive);
        }

        public override HexReactor Hex
        {
            get
            {
                return m_hex;
            }
            set
            {
                m_hex = value;
                Assert.AreEqual(m_hex.Conditions, TraversalConditions.Broken, "terrain entities are always placed over broken land to ensure that when they're destroyed there's rubble below");
            }
        }

        //inanimate objects take heat damage as physical damage
        protected override void InternalDamage(double damage, EffectType damageType)
        {
            if (damageType == EffectType.HeatDamage || damageType == EffectType.IncendiaryDamage)
            {
                damageType = EffectType.PhysicalDamage;
            }
            base.InternalDamage(damage, damageType);
        }
    }

    #endregion inanimate entities
}