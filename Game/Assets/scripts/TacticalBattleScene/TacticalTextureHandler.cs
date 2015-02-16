using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    /// <summary>
    /// Handles texture loading & replacing in the tactical battle screen
    /// </summary>
    public class TacticalTextureHandler : LoyaltyAwareTextureHandler
    {
        #region fields

        private readonly Dictionary<string, Texture2D> m_effectsTextures;
        private readonly Dictionary<string, Texture2D> m_buttonTextures;
        private readonly Dictionary<string, Texture2D> m_hexEffectsTextures;

        #endregion fields

        #region constructor

        public TacticalTextureHandler()
        {
            m_effectsTextures = GetDictionary("effects");
            m_buttonTextures = GetDictionary("SystemsUI");
            m_hexEffectsTextures = GetDictionary("HexEffects");
        }

        #endregion constructor

        #region public methods

        public void UpdateEntityTexture(EntityReactor ent)
        {
            var name = "{0}_{1}".FormatWith(ent.Loyalty, ent.Template.Name);
            var renderer = ent.GetComponent<SpriteRenderer>();
            var newTexture = GetEntityTexture(ent.Template, ent.Loyalty);
            ReplaceTexture(renderer, newTexture);
        }

        public void UpdateShotTexture(string effectName, SpriteRenderer renderer)
        {
            UpdateTexture(effectName, renderer, m_effectsTextures, "effects textures");
        }

        public void UpdateButtonTexture(string buttonName, SpriteRenderer renderer)
        {
            UpdateTexture(buttonName, renderer, m_buttonTextures, "button textures");
        }

        public void UpdateHexEffectTexture(HexEffectTemplate effectTemplate, SpriteRenderer renderer)
        {
            UpdateTexture(effectTemplate.Name, renderer, m_hexEffectsTextures, "hex effects textures");
        }

        #endregion public methods
    }
}