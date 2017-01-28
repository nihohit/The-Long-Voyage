using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Base;
using System.Linq;

namespace Assets.Scripts.StrategicGameScene
{
	public class StrategicMapTextureHandler
	{
		private readonly Dictionary<string, Sprite> m_hexes;

		public StrategicMapTextureHandler()
		{
			var textures = Resources.LoadAll<Sprite>("Hexes");
			m_hexes = textures.ToDictionary(
				texture => texture.name,
				texture => texture);
		}

		public void UpdateHexTexture(SpriteRenderer renderer, Biome biome)
		{
			renderer.sprite = m_hexes.Get(biome.ToString(), "Biomes");
		}
	}
}