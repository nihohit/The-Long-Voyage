using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.LogicBase
{
    /// <summary>
    /// Colors entities' textures, based on their loyalty
    /// </summary>
    public abstract class LoyaltyAwareTextureHandler : TextureHandler
    {
        private readonly Dictionary<string, Texture2D> r_uncoloredEntityTextures = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, Texture2D> r_knownEntityTextures = new Dictionary<string, Texture2D>();

        private readonly Dictionary<Loyalty, Color> r_affiliationColors = new Dictionary<Loyalty, Color>
        {
            { Loyalty.Bandits, Color.red },
            { Loyalty.EnemyArmy, Color.black },
            { Loyalty.Friendly, Color.yellow },
            { Loyalty.Player, Color.blue },
        }; // inactive or monster units should have unique visuals.

        protected LoyaltyAwareTextureHandler()
        {
            this.r_uncoloredEntityTextures = GetDictionary("Entities");
        }

        protected Texture2D GetEntityTexture(EntityTemplate template, Loyalty loyalty)
        {
            var name = "{0}_{1}".FormatWith(template.Name, loyalty);
            var texture = this.r_uncoloredEntityTextures.Get(template.Name, "entity textures");

            if (loyalty == Loyalty.Inactive)
            {
                return texture;
            }

            return this.r_knownEntityTextures.TryGetOrAdd(name, () => GetColoredTexture(texture, loyalty, name));
        }

        // the default switching color is white
        private Texture2D GetColoredTexture(Texture2D copiedTexture, Loyalty chosenLoyalty, string textureName)
        {
            return GetColoredTexture(copiedTexture, Color.white, this.r_affiliationColors[chosenLoyalty], textureName);
        }
    }
}