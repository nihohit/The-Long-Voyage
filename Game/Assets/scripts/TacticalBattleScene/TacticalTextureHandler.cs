using Assets.scripts.Base;
using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    public class TacticalTextureHandler : TextureHandler
    {
        #region fields

        private Dictionary<string, Texture2D> m_knownEntityTextures = new Dictionary<string, Texture2D>();
        private Dictionary<string, Texture2D> m_knownEffectsTextures;
        private Dictionary<string, Texture2D> m_knownButtonTextures;

        private Dictionary<Loyalty, Color> m_affiliationColors = new Dictionary<Loyalty, Color>
    {
        {Loyalty.Bandits, Color.red},
        {Loyalty.EnemyArmy, Color.black},
        {Loyalty.Friendly, Color.yellow},
        {Loyalty.Player, Color.blue},
    }; //inactive or monster units should have unique visuals.

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

        public void UpdateEntityTexture(TacticalEntity ent)
        {
            var name = "{0}_{1}".FormatWith(ent.Loyalty, ent.GetType().ToString());
            var renderer = ent.Reactor.GetComponent<SpriteRenderer>();
            var newTexture = m_knownEntityTextures.TryGetOrAdd(name, () => GetEntityTexture(ent, name, renderer));
            ReplaceTexture(renderer, newTexture, name);
        }

        public void UpdateEffectTexture(string effectName, SpriteRenderer renderer)
        {
            var newTexture = m_knownEffectsTextures[effectName];
            ReplaceTexture(renderer, newTexture, effectName);
        }

        public void UpdateButtonTexture(string buttonName, SpriteRenderer renderer)
        {
            var newTexture = m_knownButtonTextures[buttonName];
            ReplaceTexture(renderer, newTexture, buttonName);
        }

        #endregion public methods

        #region private methods

        private Texture2D GetEntityTexture(TacticalEntity ent, string name, SpriteRenderer renderer)
        {
            var oldTexture = ent.Reactor.GetComponent<SpriteRenderer>().sprite.texture;
            Color replacementColor;

            //if the color isn't in the list of affiliation, we just return
            if (!m_affiliationColors.TryGetValue(ent.Loyalty, out replacementColor))
            {
                return oldTexture;
            }
            return CopyTexture2D(oldTexture, replacementColor, name);
        }

        private void ReplaceTexture(SpriteRenderer renderer, Texture2D newTexture, string name)
        {
            renderer.sprite = Sprite.Create(newTexture, renderer.sprite.rect, new Vector2(0.5f, 0.5f));
            renderer.sprite.name = name;
        }

        #endregion private methods
    }
}