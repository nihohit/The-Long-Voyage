using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using Assets.Scripts.InterSceneCommunication;

namespace Assets.Scripts.StrategicGameScene.MapGeneration
{
    public class MutatingAreasMapGenerator
    {
        private Dictionary<Biome, Dictionary<Biome, double>> m_mutateToChance = new Dictionary<Biome, Dictionary<Biome, double>>
        {
            {Biome.Mountain, new Dictionary<Biome, double> {{Biome.Jungle, 0.1},{Biome.Hills, 0.3},{Biome.Forest, 0.2},{Biome.Mountain, 1.5 }, { Biome.Sea, 0.1 }, { Biome.Plains, 0.2 }, { Biome.Desert, 0.3 },  } },
            {Biome.Forest, new Dictionary<Biome, double> {{Biome.Jungle, 0.1},{Biome.Hills, 0.2},{Biome.Forest, 1.6},{Biome.Mountain, 0.1 }, { Biome.Sea, 0.1 }, { Biome.Plains, 0.2 }, { Biome.Desert, 0.05 }, } },
            {Biome.Hills, new Dictionary<Biome, double> {{Biome.Jungle, 0.1},{Biome.Hills, 1.6},{Biome.Forest, 0.1},{Biome.Mountain, 0.2 }, { Biome.Sea, 0.1 }, { Biome.Plains, 0.2 }, { Biome.Desert, 0.1 }, } },
            {Biome.Undefined, new Dictionary<Biome, double> {{Biome.Jungle, 0.3},{Biome.Hills, 0.3},{Biome.Forest, 0.3 }, { Biome.Plains, 0.3 }, { Biome.Desert, 0.3 }, } },
            {Biome.Jungle, new Dictionary<Biome, double> { {Biome.Jungle, 1.6},{Biome.Hills, 0.1},{Biome.Forest, 0.2},{Biome.Mountain, 0.1 }, { Biome.Sea, 0.1 }, { Biome.Plains, 0.1 }, { Biome.Desert, 0.01 }, } },
            {Biome.Desert, new Dictionary<Biome, double> { {Biome.Jungle, 0.01},{Biome.Hills, 0.1},{Biome.Forest, 0.05},{Biome.Mountain, 0.1 }, { Biome.Sea, 0.05 }, { Biome.Plains, 0.2 }, { Biome.Desert, 1.6 }, } },
            {Biome.Sea, new Dictionary<Biome, double> { {Biome.Jungle, 0.1},{Biome.Hills, 0.1},{Biome.Forest, 0.1},{Biome.Mountain, 0.05 }, { Biome.Sea, 1.8 }, { Biome.Plains, 0.1 }, { Biome.Desert, 0.05 }, } },
            {Biome.Plains, new Dictionary<Biome, double> { {Biome.Jungle, 0.1},{Biome.Hills, 0.2},{Biome.Forest, 0.2},{Biome.Mountain, 0.1 }, { Biome.Sea, 0.1 }, { Biome.Plains, 1.6 }, { Biome.Desert, 0.1 }, } },
        };

        private List<BiomeInformation> m_existingBiomes = new List<BiomeInformation>();
        private readonly Dictionary<Vector2, Biome> m_map = new Dictionary<Vector2, Biome>();
		private GameObject m_locationsParent;

        public LocationInformation GenerateStrategicMap()
        {
            GenerateBaseCoords();

            CreateFirstBiome();

            ExpandBiomes();

            Assert.IsEmpty<Biome>(m_map.Values.Where(biome => biome == Biome.Undefined), "undefined tiles in map");

            CreateTiles();

            return null;
        }

        private void CreateFirstBiome()
        {
            var initialBiomeType = Randomiser.ChooseWeightedValues(m_mutateToChance[Biome.Undefined],1).First();
            CreateNewBiome(m_map.Keys.ChooseRandomValue(), initialBiomeType);
        }

        private void CreateTiles()
        {
			m_locationsParent = new GameObject();
			m_locationsParent.name = "Locations";

			foreach (var tile in m_map)
            {
                var location = UnityHelper.Instantiate<LocationScript>(tile.Key);
				location.transform.SetParent(m_locationsParent.transform);
                GlobalState.Instance.StrategicMap.StrategicMapTextureHandler.UpdateHexTexture(location.Renderer, tile.Value);
				location.name = "{0} {1}".FormatWith(tile.Value.ToString(), tile.Key.ToString());
            }
        }

        private void ExpandBiomes()
        {
            while (m_existingBiomes.Any(biome => GetNeighbours(biome).Where(tile => m_map[tile] == Biome.Undefined).Any()))
            {
                var chosenBiome = m_existingBiomes.Where(biome => GetNeighbours(biome).Where(tile => m_map[tile] == Biome.Undefined).Any()).ChooseRandomValue();
                var chosenNeighbour = GetNeighbours(chosenBiome).Where(tile => m_map[tile] == Biome.Undefined).ChooseRandomValue();

                //TODO - we can weight the neighbour according to all of its neighbours, not just the chosen biome.
                var chosenBiomeType = Randomiser.ChooseWeightedValues(m_mutateToChance[chosenBiome.Biome], 1).First();
                m_map[chosenNeighbour] = chosenBiomeType;
                if (!AddToNeighbouringBiome(chosenNeighbour, chosenBiomeType))
                {
                    m_existingBiomes.Add(new BiomeInformation(chosenBiomeType, chosenNeighbour));
                }
            }
        }

        private bool AddToNeighbouringBiome(Vector2 tile, Biome biome)
        {
            var neighbours = GetNeighbours(tile);
            if(neighbours.None(neighbour => m_map[neighbour] == biome))
            {
                return false;
            }

            m_existingBiomes.First(biomeInfo => biomeInfo.Biome == biome && GetNeighbours(biomeInfo).Contains(tile)).Tiles.Add(tile);
            return true;
        }

        private void GenerateBaseCoords()
        {
            var amountOfHexes = 8;
            var target = 2 * amountOfHexes - 1;

            // create all hexes in a hexagon shape - this creates the top half
            //the math became a bit complicated when trying to account for correct coordinates.
            for (int i = -amountOfHexes + 1; i <= 0; i++)
            {
                var amountOfHexesInRow = i + target;
                var entryCoordinate = (float)-i / 2 - amountOfHexes + 1;
                for (float j = 0; j < amountOfHexesInRow; j++)
                {
                    m_map.Add(new Vector2(entryCoordinate + j, i), Biome.Undefined);
                }
            }

            // create the bottom half of the hexagon
            for (int i = 1; i < target - amountOfHexes + 1; i++)
            {
                var amountOfHexesInRow = target - i;
                var entryCoordinate = (float)i / 2 - amountOfHexes + 1;
                for (float j = 0; j < amountOfHexesInRow; j++)
                {
                    m_map.Add(new Vector2(entryCoordinate + j, i), Biome.Undefined);
                }
            }
        }

        private BiomeInformation CreateNewBiome(Vector2 tile, Biome biome)
        {
            m_map[tile] = biome;
            var newBiome = new BiomeInformation(biome, tile);
            m_existingBiomes.Add(newBiome);
            return newBiome;
        }

        private IEnumerable<Vector2> GetNeighbours(BiomeInformation biome)
        {
            return biome.Tiles.SelectMany(tile => GetNeighbours(tile)).Distinct();
        }

        private IEnumerable<Vector2> GetNeighbours(Vector2 tile)
        {
            var result = new List<Vector2>();
            CheckAndAdd(result, new Vector2(tile.x - 0.5f, tile.y - 1));
            CheckAndAdd(result, new Vector2(tile.x + 0.5f, tile.y - 1));
            CheckAndAdd(result, new Vector2(tile.x + 1.0f, tile.y));
            CheckAndAdd(result, new Vector2(tile.x - 1.0f, tile.y));
            CheckAndAdd(result, new Vector2(tile.x - 0.5f, tile.y + 1));
            CheckAndAdd(result, new Vector2(tile.x + 0.5f, tile.y + 1));
            return result;
        }

        private void CheckAndAdd(IList<Vector2> result, Vector2 coordinates)
        {
            Biome temp;
            if (m_map.TryGetValue(coordinates, out temp))
            {
                result.Add(coordinates);
            }
        }

        private class BiomeInformation
        {
            public Biome Biome { get; private set; }
            public List<Vector2> Tiles { get; private set; }

            public BiomeInformation(Biome biome, Vector2 firstTile)
            {
                Biome = biome;
                Tiles = new List<Vector2> { firstTile };
            }
        }
    }
}
