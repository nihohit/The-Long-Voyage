﻿using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Assets.Scripts.TacticalBattleScene
{
    /// <summary>
    /// The main script for the tactical battle scene
    /// </summary>
    public class TacticalBattleScript : MonoBehaviour
    {
        #region private members

        //HACK - to be deleted.
        private readonly List<HexReactor> m_emptyHexes = new List<HexReactor>();

        private int m_screenSpeed;

		private GameObject m_hexes;
		private GameObject m_terrainEntities;
		private GameObject m_activeEntities;

        #endregion private members

        #region public members

        // TODO - get rid of these
        public GameObject greenHex;

        public GameObject woodHex;
        public Camera mainCamera;

        #endregion public members

        #region MonoBehaviour overrides

        // runs on every frame
        private void Update()
        {
            //the first time that update is called, start the battle.
            // This has to be so, because otherwise the turn might start before all the hexes have started.
            if (!TacticalState.BattleStarted)
            {
                TacticalState.BattleStarted = true;
                TacticalState.StartTurn();
            }

            // update camera position based on input. The axis are defined in the Unity editor
            if (Camera.current != null)
            {
                float xAxisValue = Input.GetAxis("Horizontal") * m_screenSpeed;
                float yAxisValue = Input.GetAxis("Vertical") * m_screenSpeed;
                float zAxisValue = Input.GetAxisRaw("Zoom");
                Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
            }

            // if right mouse button is pressed
            if (Input.GetMouseButton(1))
            {
                TacticalState.SelectedHex = null;
            }
        }

        // Use this for initialization
        private void Start()
        {
			FindBaseObjects();
            InitClasses();
            m_screenSpeed = SimpleConfigurationHandler.GetIntProperty("screen movement speed", FileAccessor.General);

            // create new hexes from a given entry point and of a given size
            var hexes = new List<HexReactor>();
            var entryPoint = Vector3.zero;
            var hexSize = greenHex.GetComponent<Renderer>().bounds.size;

            // reset the global state's tactical battle information
            var state = GlobalState.Instance.TacticalBattle;
            GlobalState.Instance.TacticalBattle = null;

            var target = 2 * state.AmountOfHexes - 1;

            // create all hexes in a hexagon shape - this creates the top half
            //the math became a bit complicated when trying to account for correct coordinates.
            for (int i = -state.AmountOfHexes + 1; i <= 0; i++)
            {
                entryPoint = new Vector3(entryPoint.x - (hexSize.x / 2), entryPoint.y + (hexSize.x * Mathf.Sqrt(3) / 2), entryPoint.z);
                var amountOfHexesInRow = i + target;
                var entryCoordinate = (float)-i / 2 - state.AmountOfHexes + 1;
                for (float j = 0; j < amountOfHexesInRow; j++)
                {
					if (entryCoordinate + j == 0 && i == 0)
					{
						hexes.Add(CreateGrassHex(
							new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
							new Vector2(entryCoordinate + j, i)));
					}
					else
					{
						hexes.Add(CreateLightTreesHex(
							new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
							new Vector2(entryCoordinate + j, i)));
					}
                }
            }

            // center the camera at the center of the hexagon
            mainCamera.transform.position = new Vector3(entryPoint.x + ((state.AmountOfHexes - 1) * hexSize.x), entryPoint.y, entryPoint.z - 70);
            mainCamera.transform.Rotate(new Vector3(180, 180, 180));

            // create the bottom half of the hexagon
            for (int i = 1; i < target - state.AmountOfHexes + 1; i++)
            {
                entryPoint = new Vector3(entryPoint.x + (hexSize.x / 2), entryPoint.y + (hexSize.x * Mathf.Sqrt(3) / 2), entryPoint.z);
                var amountOfHexesInRow = target - i;
                var entryCoordinate = (float)i / 2 - state.AmountOfHexes + 1;
                for (float j = 0; j < amountOfHexesInRow; j++)
                {
                    hexes.Add(CreateLightTreesHex(
                        new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
                        new Vector2(entryCoordinate + j, i)));
                }
            }

            // inititate the tactical state
            TacticalState.EnterEntitiesAndHexes(state.EntitiesInBattle, hexes);

            // position the entities
            //HACK - to be removed. in charge of positioning the first entities
            var chosenHexes = m_emptyHexes.ChooseRandomValues(state.EntitiesInBattle.Count()).OrderBy(x => Randomiser.Next());
            chosenHexes.ForEach(hex => hex.Content = state.EntitiesInBattle.First(ent => ent.Hex == null));
        }

		private void FindBaseObjects()
		{
			m_hexes = GameObject.Find("Hexes");
			m_terrainEntities = GameObject.Find("TerrainEntities");
			m_activeEntities = GameObject.Find("ActiveEntities");
		}

		#endregion MonoBehaviour overrides

		#region private methods

		private void InitClasses()
        {
            TacticalState.Init();
            InitiateGlobalState();
        }

        private HexReactor CreateGrassHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var reactor = CreateHex(nextPosition, hexCoordinates, greenHex);
            m_emptyHexes.Add(reactor);
            return reactor;
        }

        private HexReactor CreateLightTreesHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            return CreateWoodHex(nextPosition, hexCoordinates, "SparseTrees");
        }

        private HexReactor CreateDenseTreesHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            return CreateWoodHex(nextPosition, hexCoordinates, "DenseTrees");
        }

        private HexReactor CreateBuildingHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            return CreateWoodHex(nextPosition, hexCoordinates, "Building");
        }

        private HexReactor CreateWoodHex(Vector3 nextPosition, Vector2 hexCoordinates, string configurationName)
        {
            var reactor = CreateHex(nextPosition, hexCoordinates, woodHex);
            reactor.Conditions = TraversalConditions.Broken;
            var terrainEntity = UnityHelper.Instantiate<TerrainEntity>(transform.position);
            reactor.Content = terrainEntity;
            terrainEntity.Init(GlobalState.Instance.Configurations.TerrainEntities.GetConfiguration(configurationName));
			terrainEntity.transform.SetParent(m_terrainEntities.transform);
            return reactor;
        }

        private HexReactor CreateHex(Vector3 nextPosition, Vector2 hexCoordinates, GameObject prefab)
        {
            // TODO - remove usage of prefab and use UnityHelper instead
            var hex = (GameObject)Instantiate(prefab, nextPosition, Quaternion.identity);
            var reactor = hex.GetComponent<HexReactor>();
            reactor.Init(hexCoordinates);
			reactor.transform.SetParent(m_hexes.transform);
            return reactor;
        }

        private HexReactor CreateRandomHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var random = Randomiser.Next(1, 8);
            switch (random)
            {
                case (1):
                    return CreateLightTreesHex(nextPosition, hexCoordinates);

                case (2):
                    return CreateDenseTreesHex(nextPosition, hexCoordinates);

                case (3):
                    return CreateBuildingHex(nextPosition, hexCoordinates);

                default:
                    return CreateGrassHex(nextPosition, hexCoordinates);
            }
        }

        // initiate all relevant classes and create a new global state if there's no current one
        private void InitiateGlobalState()
        {
            HexReactor.Init();

            //TODO - replace with exception throwing when we remove the direct access to level generation
            if (GlobalState.Instance.TacticalBattle == null)
            {
				var state = new TacticalBattleInformation
				{
					AmountOfHexes = SimpleConfigurationHandler.GetIntProperty(
							"default map size",
							FileAccessor.TerrainGeneration),
					EntitiesInBattle = CreateMechs(new[] { new EquippedEntity(new SpecificEntity("ScoutMech"), new[] { "Flamer","Laser","Missile" })}, Loyalty.Player)
                    };
                GlobalState.Instance.TacticalBattle = state;
            }
        }

        // create a collection of mechs
        private IEnumerable<ActiveEntity> CreateMechs(Loyalty loyalty, int number)
        {
            return CreateMechs(
                Enumerable.Range(0, number).Select(num =>
                {
                    var template = GlobalState.Instance.Configurations.ActiveEntities.GetAllConfigurations().ChooseRandomValue();
                    var systems = GlobalState.Instance.Configurations.Subsystems.GetAllConfigurations().ChooseRandomValues(template.SystemSlots);
                    return new EquippedEntity(new SpecificEntity(template), systems);
                }),
                loyalty);
        }

        // create the player controlled mechs from their definition in the global state
        private IEnumerable<ActiveEntity> CreateMechs(IEnumerable<EquippedEntity> equippedEntities, Loyalty loyalty)
        {
            return equippedEntities.Select(equippedEntity =>
                {
                    var entity = (ActiveEntity)UnityHelper.Instantiate<MovingEntity>(transform.position);
                    entity.Init(
                        equippedEntity.InternalEntity,
                        loyalty,
                        equippedEntity.Subsystems);
					entity.transform.SetParent(m_activeEntities.transform);
                    return entity;
                }).Materialize();
        }

        #endregion private methods
    }
}