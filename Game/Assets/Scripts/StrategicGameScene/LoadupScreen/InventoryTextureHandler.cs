using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene.LoadupScreen
{
    /// <summary>
    /// Texture handler for the inventory screen
    /// </summary>
    public class InventoryTextureHandler : LoyaltyAwareTextureHandler, ITextureHandler<SpecificEntity>, ITextureHandler<SubsystemTemplate>
    {
        #region fields

        private readonly Dictionary<string, Texture2D> r_buttonTextures;

        #endregion fields

        #region constructor

        public InventoryTextureHandler()
        {
            this.r_buttonTextures = GetDictionary("SystemsUI");
        }

        #endregion constructor

        #region public methods

        public void UpdateMarkerTexture(SpecificEntity ent, SpriteRenderer renderer)
        {
            ReplaceTexture(renderer, GetEntityTexture(ent.Template, Loyalty.Player));
        }

        public void UpdateMarkerTexture(SubsystemTemplate item, SpriteRenderer renderer)
        {
            UpdateTexture(item.Name, renderer, this.r_buttonTextures, "button textures");
        }

        public Texture2D GetTexture(SpecificEntity ent)
        {
            return GetEntityTexture(ent.Template, Loyalty.Player);
        }

        public Texture2D GetTexture(SubsystemTemplate system)
        {
            return this.r_buttonTextures.Get(system.Name, "button textures");
        }

        public Texture2D GetNullTexture()
        {
            return this.r_buttonTextures.Get("Null");
        }

        public Texture2D GetEmptyTexture()
        {
            return this.r_buttonTextures.Get("Empty");
        }

        #endregion public methods
    }
}