﻿using Assets.scripts.Base;
using Assets.scripts.InterSceneCommunication;
using Assets.scripts.LogicBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    public class TacticalBattleScript : MonoBehaviour
    {
        #region public members

        public GameObject greenHex;
        public GameObject woodHex;
        public Camera mainCamera;

        #endregion public members

        //HACK - to be deleted.
        private List<Hex> m_emptyHexes = new List<Hex>();

        #region MonoBehaviour overrides

        private void Update()
        {
            //the first time that update is called, start the battle.
            // This has to be so, because otherwise the turn might start before all the hexes have started.
            if (!TacticalState.BattleStarted)
            {
                TacticalState.BattleStarted = true;
                TacticalState.StartTurn();
            }

            if (Camera.current != null)
            {
                float xAxisValue = Input.GetAxis("Horizontal");
                float yAxisValue = Input.GetAxis("Vertical");
                float zAxisValue = Input.GetAxisRaw("Zoom");
                Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
            }
            if (Input.GetMouseButton(1))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content == null)
                    {
                        //TODO - this is just a temporary measure, to create mechs
                        var loyalty = TacticalState.CurrentTurn;
                        var mech = new MovingEntity(
                            EntityTemplate.GetTemplate(0),
                            loyalty,
                            ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>(),
                            new Subsystem[] { new Laser(loyalty), new MissileLauncher(loyalty), new EmpLauncher(loyalty), new HeatWaveProjector(loyalty), new IncediaryGun(loyalty) });
                        TacticalState.SelectedHex.MarkedHex.Content = mech;
                        TacticalState.AddEntity(mech);
                        Debug.Log("created {0} at {1}".FormatWith(mech, TacticalState.SelectedHex));
                    }
                    TacticalState.SelectedHex = null;
                }
                else
                {
                    if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content != null)
                    {
                        Debug.Log(TacticalState.SelectedHex.MarkedHex.Content);
                    }
                    TacticalState.SelectedHex = null;
                }
            }
        }

        // Use this for initialization
        private void Start()
        {
            InitClasses();

            var hexes = new List<Hex>();
            var entryPoint = Vector3.zero;
            var hexSize = greenHex.renderer.bounds.size;
            var state = GlobalState.TacticalBattle;
            GlobalState.TacticalBattle = null;

            var target = 2 * state.AmountOfHexes - 1;

            //the math became a bit complicated when trying to account for correct coordinates.
            for (int i = -state.AmountOfHexes + 1; i <= 0; i++)
            {
                entryPoint = new Vector3(entryPoint.x - (hexSize.x / 2), entryPoint.y + (hexSize.x * Mathf.Sqrt(3) / 2), entryPoint.z);
                var amountOfHexesInRow = i + target;
                var entryCoordinate = (float)-i / 2 - state.AmountOfHexes + 1;
                for (float j = 0; j < amountOfHexesInRow; j++)
                {
                    hexes.Add(CreateRandomHex(
                        new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
                        new Vector2(entryCoordinate + j, i)).MarkedHex);
                }
            }

            mainCamera.transform.position = new Vector3(entryPoint.x + ((state.AmountOfHexes - 1) * hexSize.x), entryPoint.y, entryPoint.z - 70);
            mainCamera.transform.Rotate(new Vector3(180, 180, 180));

            for (int i = 1; i < target - state.AmountOfHexes + 1; i++)
            {
                entryPoint = new Vector3(entryPoint.x + (hexSize.x / 2), entryPoint.y + (hexSize.x * Mathf.Sqrt(3) / 2), entryPoint.z);
                var amountOfHexesInRow = target - i;
                var entryCoordinate = (float)i / 2 - state.AmountOfHexes + 1;
                for (float j = 0; j < amountOfHexesInRow; j++)
                {
                    hexes.Add(CreateRandomHex(
                        new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
                        new Vector2(entryCoordinate + j, i)).MarkedHex);
                }
            }

            TacticalState.Init(state.EntitiesInBattle, hexes);

            //HACK - to be removed. in charge of positioning the first entities
            var chosenHexes = m_emptyHexes.ChooseRandomValues(state.EntitiesInBattle.Count()).OrderBy(x => Randomiser.Next());
            chosenHexes.ForEach(hex => hex.Content = state.EntitiesInBattle.First(ent => ent.Hex == null));
        }

        private void InitClasses()
        {
            TacticalState.BattleStarted = false;
            InitiateGlobalState();
        }

        #endregion MonoBehaviour overrides

        #region private methods

        private HexReactor CreateGrassHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var reactor = CreateHex(nextPosition, hexCoordinates, greenHex);
            m_emptyHexes.Add(reactor.MarkedHex);
            return reactor;
        }

        private HexReactor CreateLightTreesHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var hex = CreateWoodHex(nextPosition, hexCoordinates);
            hex.MarkedHex.Content = new TerrainEntity(EntityTemplate.GetTemplate(3),
                ((GameObject)Instantiate(Resources.Load("SparseTrees"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
            return hex;
        }

        private HexReactor CreateDenseTreesHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var hex = CreateWoodHex(nextPosition, hexCoordinates);
            hex.MarkedHex.Content = new TerrainEntity(EntityTemplate.GetTemplate(2),
                ((GameObject)Instantiate(Resources.Load("DenseTrees"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
            return hex;
        }

        private HexReactor CreateBuildingHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var hex = CreateWoodHex(nextPosition, hexCoordinates);
            hex.MarkedHex.Content = new TerrainEntity(EntityTemplate.GetTemplate(4),
                ((GameObject)Instantiate(Resources.Load("Building"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
            return hex;
        }

        private HexReactor CreateWoodHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var reactor = CreateHex(nextPosition, hexCoordinates, woodHex);
            reactor.MarkedHex.Conditions = TraversalConditions.Broken;
            return reactor;
        }

        private HexReactor CreateHex(Vector3 nextPosition, Vector2 hexCoordinates, GameObject prefab)
        {
            var hex = (GameObject)Instantiate(prefab, nextPosition, Quaternion.identity);
            var reactor = hex.GetComponent<HexReactor>();
            reactor.MarkedHex = new Hex(hexCoordinates, reactor);
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

        private void CreateRandomHex(Vector2 hexCoordinates)
        {
            CreateRandomHex(Vector3.zero, hexCoordinates);
        }

        private float GetDistance(Vector3 origin, Vector3 target)
        {
            return Mathf.Sqrt(Mathf.Pow(origin.x - target.x, 2f) + Mathf.Pow(origin.y - target.y, 2f) + Mathf.Pow(origin.z - target.z, 2f));
        }

        private void InitiateGlobalState()
        {
            //TODO - replace with exception throwing when we remove the direct access to level generation
            FileHandler.Init();
            HexReactor.Init();
            SubsystemTemplate.Init();
            EntityTemplate.Init();
            Hex.Init();
            
            if (GlobalState.TacticalBattle == null)
            {
                var state = new TacticalBattleInformation();
                state.AmountOfHexes = FileHandler.GetIntProperty(
                    "default map size",
                    FileAccessor.TerrainGeneration);
                state.EntitiesInBattle = CreateMechs(Loyalty.EnemyArmy, 4).Union(CreateMechs(Loyalty.Player, 4));
                GlobalState.TacticalBattle = state;
            }
        }

        private IEnumerable<ActiveEntity> CreateMechs(Loyalty loyalty, int number)
        {
            return Enumerable.Range(0, number).Select(num => (ActiveEntity)new MovingEntity(
                EntityTemplate.GetTemplate(1),
                loyalty,
                ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>(),
                new Subsystem[] {
                new Laser(loyalty),
                new MissileLauncher(loyalty),
                new EmpLauncher(loyalty),
                new HeatWaveProjector(loyalty),
                new IncediaryGun(loyalty) })
                ).Materialize();
        }

        #endregion private methods
    }
}