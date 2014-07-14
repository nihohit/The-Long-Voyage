﻿using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.InventoryScreen
{
    public class InventoryTextureHandler : LoyaltyAwareTextureHandler
    {
        #region fields

        private Dictionary<string, Texture2D> m_knownEntityTextures;
        private Dictionary<string, Texture2D> m_knownButtonTextures;
        private Texture2D m_nullTexture;

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
            m_nullTexture = Resources.Load<Texture2D>(@"UI\Null.png");
        }

        #endregion constructor

        #region public methods

        public void UpdateEntityMarkerTexture(SpecificEntity ent, SpriteRenderer renderer)
        {
            var texture = m_knownEntityTextures[ent.Template.Name];
            ReplaceTexture(renderer, GetEntityTexture(ent.Template, Loyalty.Player, texture), ent.Template.Name);
        }

        public Texture2D GetEntityTexture(SpecificEntity ent)
        {
            var texture = m_knownEntityTextures[ent.Template.Name];
            return GetEntityTexture(ent.Template, Loyalty.Player, texture);
        }

        public Texture2D GetSystemTexture(SubsystemTemplate system)
        {
            return m_knownButtonTextures[system.Name];
        }

        public Texture2D GetNullTexture()
        {
            return m_nullTexture;
        }

        #endregion public methods
    }
}
