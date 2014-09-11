using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.InventoryScreen
{
    /// <summary>
    /// Texture handler for the inventory screen
    /// </summary>
    public class InventoryTextureHandler : LoyaltyAwareTextureHandler, ITextureHandler<SpecificEntity>, ITextureHandler<SubsystemTemplate>
    {
        #region fields

        private Dictionary<string, Texture2D> m_knownEntityTextures;
        private Dictionary<string, Texture2D> m_knownButtonTextures;

        #endregion fields

        #region constructor

        public InventoryTextureHandler()
        {
            var textures = Resources.LoadAll<Texture2D>("Entities");
            m_knownEntityTextures = textures.ToDictionary(texture => texture.name,
                                                          texture => texture);
            textures = Resources.LoadAll<Texture2D>("UI");
            m_knownButtonTextures = textures.ToDictionary(texture => texture.name,
                                                           texture => texture);
        }

        #endregion constructor

        #region public methods

        public void UpdateMarkerTexture(SpecificEntity ent, SpriteRenderer renderer)
        {
            var texture = m_knownEntityTextures.Get(ent.Template.Name, "entity textures");
            ReplaceTexture(renderer, GetEntityTexture(ent.Template, Loyalty.Player, texture), ent.Template.Name);
        }

        public void UpdateMarkerTexture(SubsystemTemplate item, SpriteRenderer renderer)
        {
            var texture = m_knownButtonTextures.Get(item.Name, "button textures");
            ReplaceTexture(renderer, texture, item.Name);
        }

        public Texture2D GetTexture(SpecificEntity ent)
        {
            var texture = m_knownEntityTextures.Get(ent.Template.Name, "entity textures");
            return GetEntityTexture(ent.Template, Loyalty.Player, texture);
        }

        public Texture2D GetTexture(SubsystemTemplate system)
        {
            return m_knownButtonTextures.Get(system.Name, "button textures");
        }

        public Texture2D GetNullTexture()
        {
            return m_knownButtonTextures.Get("Null");
        }

        #endregion public methods
    }
}