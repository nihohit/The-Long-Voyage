using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.LogicBase
{
    /// <summary>
    /// Colors entities' textures, based on their loyalty
    /// </summary>
    public abstract class LoyaltyAwareTextureHandler : TextureHandler
    {
        private Dictionary<string, Texture2D> m_uncoloredEntityTextures = new Dictionary<string, Texture2D>();
        private Dictionary<string, Texture2D> m_knownEntityTextures = new Dictionary<string, Texture2D>();

        private Dictionary<Loyalty, Color> m_affiliationColors = new Dictionary<Loyalty, Color>
        {
        {Loyalty.Bandits, Color.red},
        {Loyalty.EnemyArmy, Color.black},
        {Loyalty.Friendly, Color.yellow},
        {Loyalty.Player, Color.blue},
        }; //inactive or monster units should have unique visuals.

        public LoyaltyAwareTextureHandler()
        {
            var textures = Resources.LoadAll<Texture2D>("Entities");
            m_uncoloredEntityTextures = textures.ToDictionary(texture => texture.name,
                                                          texture => texture);
        }

        protected Texture2D GetEntityTexture(EntityTemplate template, Loyalty loyalty)
        {
            var name = "{0}_{1}".FormatWith(template.Name, loyalty);
            var texture = m_uncoloredEntityTextures.Get(template.Name, "entity textures");
            return m_knownEntityTextures.TryGetOrAdd(name, () => GetColoredTexture(texture, loyalty, name));
        }

        //the default switching color is white
        private Texture2D GetColoredTexture(Texture2D copiedTexture, Loyalty chosenLoyalty, string textureName)
        {
            return GetColoredTexture(copiedTexture, Color.white, m_affiliationColors[chosenLoyalty], textureName);
        }
    }
}