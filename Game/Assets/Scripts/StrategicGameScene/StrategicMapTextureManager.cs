using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.StrategicGameScene
{
    public class StrategicMapTextureHandler : TextureHandler
    {
        private readonly Dictionary<string, Texture2D> m_hexes;

        public StrategicMapTextureHandler()
        {
            m_hexes = base.GetDictionary("Hexes");
        }

        public void UpdateHexTexture(SpriteRenderer renderer, Biome biome)
        {
            base.UpdateTexture(biome.ToString(), renderer, m_hexes, "hexes");
        }
    }
}