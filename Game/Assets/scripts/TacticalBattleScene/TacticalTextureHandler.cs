using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    /// <summary>
    /// Handles texture loading & replacing in the tactical battle screen
    /// </summary>
    public class TacticalTextureHandler : LoyaltyAwareTextureHandler
    {
        #region fields

        private Dictionary<string, Texture2D> m_knownEffectsTextures;
        private Dictionary<string, Texture2D> m_knownButtonTextures;

        #endregion fields

        #region constructor

        public TacticalTextureHandler()
        {
            var textures = Resources.LoadAll<Texture2D>("effects");
            m_knownEffectsTextures = textures.ToDictionary(texture => texture.name,
                                                           texture => texture);
            textures = Resources.LoadAll<Texture2D>("UI");
            m_knownButtonTextures = textures.ToDictionary(texture => texture.name,
                                                           texture => texture);
        }

        #endregion constructor

        #region public methods

        public void UpdateEntityTexture(EntityReactor ent)
        {
            var name = "{0}_{1}".FormatWith(ent.Loyalty, ent.Template.Name);
            var renderer = ent.GetComponent<SpriteRenderer>();
            var newTexture = GetEntityTexture(ent.Template, ent.Loyalty);
            ReplaceTexture(renderer, newTexture, name);
        }

        public void UpdateEffectTexture(string effectName, SpriteRenderer renderer)
        {
            var newTexture = m_knownEffectsTextures.Get(effectName, "effects textures");
            ReplaceTexture(renderer, newTexture, effectName);
        }

        public void UpdateButtonTexture(string buttonName, SpriteRenderer renderer)
        {
            var newTexture = m_knownButtonTextures.Get(buttonName, "button textures");
            ReplaceTexture(renderer, newTexture, buttonName);
        }

        #endregion public methods
    }
}