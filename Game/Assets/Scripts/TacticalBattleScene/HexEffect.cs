using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    #region HexEffect

    public class HexEffect : MarkerScript
    {
        #region private fields

        private HexReactor m_affectedHex;

        private int m_remainingDuration;

        private static List<HexEffect> s_effects = new List<HexEffect>();

        #endregion private fields

        #region properties

        public HexEffectTemplate Template { get; private set; }

        #endregion

        #region constructors

        public void Init(HexEffectTemplate template, HexReactor hex)
        {
            Template = template;
            m_remainingDuration = template.Duration;
            m_affectedHex = hex;
        }

        #endregion constructors

        public void AffectEntity(EntityReactor entity)
        {
            entity.Affect(Template.Power, Template.EffectType);
        }

        public bool Act()
        {
            Assert.Greater(m_remainingDuration, 0);

            if(m_affectedHex.Content != null)
            {
                AffectEntity(m_affectedHex.Content);
            }

            if (--m_remainingDuration > 0) return false;

            DestroyGameObject();

            return true;
        }

        #region static methods

        public static void Create(HexEffectTemplate hexEffectTemplate, HexReactor hex)
        {
            var newEffect = ((GameObject)Instantiate(Resources.Load("HexEffect"), hex.transform.position, Quaternion.identity)).GetComponent<HexEffect>();
            newEffect.Init(hexEffectTemplate, hex);
            s_effects.Add(newEffect); 
        }

        public static void OperateEffects()
        {
            foreach(var effect in s_effects.Duplicate())
            {
                if(effect.Act())
                {
                    s_effects.Remove(effect);
                }
            }
        }

        public static void Clear()
        {
            s_effects.ForEach(effect => effect.DestroyGameObject());
            s_effects.Clear();
        }

        #endregion
    }

    #endregion HexEffect
}