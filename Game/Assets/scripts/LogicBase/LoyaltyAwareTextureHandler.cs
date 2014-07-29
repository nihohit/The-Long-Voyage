using Assets.scripts.Base;
using Assets.scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts.LogicBase
{
    /// <summary>
    /// Colors entities' textures, based on their loyalty
    /// </summary>
    public class LoyaltyAwareTextureHandler : TextureHandler
    {
        private Dictionary<string, Texture2D> m_knownEntityTextures = new Dictionary<string, Texture2D>();

        private Dictionary<Loyalty, Color> m_affiliationColors = new Dictionary<Loyalty, Color>
        {
        {Loyalty.Bandits, Color.red},
        {Loyalty.EnemyArmy, Color.black},
        {Loyalty.Friendly, Color.yellow},
        {Loyalty.Player, Color.blue},
        }; //inactive or monster units should have unique visuals.

        protected Texture2D GetEntityTexture(EntityTemplate template, Loyalty loyalty, Texture2D texture)
        {
            var name = "{0}_{1}".FormatWith(template.Name, loyalty);
            return m_knownEntityTextures.TryGetOrAdd(name, () => GetColoredTexture(texture, loyalty, name));
        }

        //the default switching color is white
        private Texture2D GetColoredTexture(Texture2D copiedTexture, Loyalty chosenLoyalty, string textureName)
        {
            return GetColoredTexture(copiedTexture, Color.white, m_affiliationColors[chosenLoyalty], textureName);
        }
    }
}