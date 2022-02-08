using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene {
  #region HexEffect

  public abstract class HexEffect : MarkerScript {
    #region private fields

    private HexReactor m_affectedHex;
    private static readonly List<HexEffect> s_effects = new List<HexEffect>();

    #endregion private fields

    #region properties

    public HexEffectTemplate Template { get; private set; }

    #endregion properties

    #region constructors

    public void Init(HexEffectTemplate template, HexReactor hex) {
      Template = template;
      m_affectedHex = hex;
      this.transform.SetParent(hex.transform);
      this.gameObject.name = template.Name;
    }

    #endregion constructors

    public void AffectEntity(EntityReactor entity) {
      entity.Affect(Template.Power, Template.EffectType);
    }

    public bool Act() {
      //Debug.Log("{0} acts with remaining duration {1}".FormatWith(Template.Name, m_remainingDuration));

      //Assert.Greater(m_remainingDuration, 0);

      if (m_affectedHex.Content != null) {
        AffectEntity(m_affectedHex.Content);
      }
      AffectNeighbours();

      //if (--m_remainingDuration > 0) return false;

      DestroyGameObject();

      return true;
    }

    private void AffectNeighbours() {

    }

    #region static methods

    public static void Create(HexEffectTemplate hexEffectTemplate, HexReactor hex) {
      var newEffect = UnityHelper.Instantiate<HexEffect>(hex.transform.position);
      newEffect.Init(hexEffectTemplate, hex);
      s_effects.Add(newEffect);
      TacticalState.TextureManager.UpdateHexEffectTexture(hexEffectTemplate, newEffect.GetComponent<SpriteRenderer>());
    }

    public static void OperateEffects() {
      Debug.Log("Operate effects");
      foreach (var effect in s_effects.Duplicate()) {
        if (effect.Act()) {
          s_effects.Remove(effect);
        }
      }
    }

    public static void Clear() {
      s_effects.ForEach(effect => effect.DestroyGameObject());
      s_effects.Clear();
    }

    #endregion static methods
  }

  #endregion HexEffect
}