using System.Collections.Generic;
using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using UnityEngine;

namespace Assets.Scripts.InventoryScreen
{
    /// <summary>
    /// Texture handler for the inventory screen
    /// </summary>
    public class InventoryTextureHandler : LoyaltyAwareTextureHandler, ITextureHandler<SpecificEntity>, ITextureHandler<SubsystemTemplate>
    {
        #region fields

        private readonly Dictionary<string, Texture2D> m_buttonTextures;

        #endregion fields

        #region constructor

        public InventoryTextureHandler()
        {
            m_buttonTextures = GetDictionary("SystemsUI");
        }

        #endregion constructor

        #region public methods

        public void UpdateMarkerTexture(SpecificEntity ent, SpriteRenderer renderer)
        {
            ReplaceTexture(renderer, GetEntityTexture(ent.Template, Loyalty.Player), ent.Template.Name);
        }

        public void UpdateMarkerTexture(SubsystemTemplate item, SpriteRenderer renderer)
        {
            UpdateTexture(item.Name, renderer, m_buttonTextures, "button textures");
        }

        public Texture2D GetTexture(SpecificEntity ent)
        {
            return GetEntityTexture(ent.Template, Loyalty.Player);
        }

        public Texture2D GetTexture(SubsystemTemplate system)
        {
            return m_buttonTextures.Get(system.Name, "button textures");
        }

        public Texture2D GetNullTexture()
        {
            return m_buttonTextures.Get("Null");
        }

        #endregion public methods
    }
}