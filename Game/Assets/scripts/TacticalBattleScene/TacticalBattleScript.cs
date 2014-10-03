using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.LogicBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    /// <summary>
    /// The main script for the tactical battle scene
    /// </summary>
    public class TacticalBattleScript : MonoBehaviour
    {
        private TerrainEntityTemplateStorage m_terrainEntities = new TerrainEntityTemplateStorage("TerrainEntities");

        #region public members

        // TODO - get rid of these
        public GameObject greenHex;

        public GameObject woodHex;
        public Camera mainCamera;

        #endregion public members

        //HACK - to be deleted.
        private List<Hex> m_emptyHexes = new List<Hex>();

        private int screenSpeed;

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
                float xAxisValue = Input.GetAxis("Horizontal") * screenSpeed;
                float yAxisValue = Input.GetAxis("Vertical") * screenSpeed;
                float zAxisValue = Input.GetAxisRaw("Zoom");
                Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
            }

            // if right mouse button is pressed
            if (Input.GetMouseButton(1))
            {
                if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content != null)
                {
                    Debug.Log(TacticalState.SelectedHex.MarkedHex.Content);
                }
                TacticalState.SelectedHex = null;
            }
        }

        // Use this for initialization
        private void Start()
        {
            InitClasses();
            screenSpeed = SimpleConfigurationHandler.GetIntProperty("screen movement speed", FileAccessor.General);

            // create new hexes from a given entry point and of a given size
            var hexes = new List<Hex>();
            var entryPoint = Vector3.zero;
            var hexSize = greenHex.renderer.bounds.size;

            // reset the global state's tactical battle information
            var state = GlobalState.TacticalBattle;
            GlobalState.TacticalBattle = null;

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
                    hexes.Add(CreateRandomHex(
                        new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
                        new Vector2(entryCoordinate + j, i)).MarkedHex);
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
                    hexes.Add(CreateRandomHex(
                        new Vector3(entryPoint.x + j * hexSize.x, entryPoint.y, entryPoint.z),
                        new Vector2(entryCoordinate + j, i)).MarkedHex);
                }
            }

            // inititate the tactical state
            TacticalState.Init(state.EntitiesInBattle, hexes);

            // position the entities
            //HACK - to be removed. in charge of positioning the first entities
            var chosenHexes = m_emptyHexes.ChooseRandomValues(state.EntitiesInBattle.Count()).OrderBy(x => Randomiser.Next());
            chosenHexes.ForEach(hex => hex.Content = state.EntitiesInBattle.First(ent => ent.Hex == null));
        }

        #endregion MonoBehaviour overrides

        #region private methods

        private void InitClasses()
        {
            TacticalState.BattleStarted = false;
            SimpleConfigurationHandler.Init();
            InitiateGlobalState();
        }

        private HexReactor CreateGrassHex(Vector3 nextPosition, Vector2 hexCoordinates)
        {
            var reactor = CreateHex(nextPosition, hexCoordinates, greenHex);
            m_emptyHexes.Add(reactor.MarkedHex);
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
            reactor.MarkedHex.Conditions = TraversalConditions.Broken;
            reactor.MarkedHex.Content = new TerrainEntity(m_terrainEntities.GetConfiguration(configurationName),
                ((GameObject)Instantiate(Resources.Load(configurationName), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
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

        // initiate all relevant classes and create a new global state if there's no current one
        private void InitiateGlobalState()
        {
            //TODO - replace with exception throwing when we remove the direct access to level generation
            SimpleConfigurationHandler.Init();
            HexReactor.Init();
            GlobalState.Init();
            Hex.Init();

            if (GlobalState.TacticalBattle == null)
            {
                var state = new TacticalBattleInformation();
                state.AmountOfHexes = SimpleConfigurationHandler.GetIntProperty(
                    "default map size",
                    FileAccessor.TerrainGeneration);
                if (GlobalState.StrategicMap == null)
                    state.EntitiesInBattle = CreateMechs(Loyalty.EnemyArmy, 4).Union(CreateMechs(Loyalty.Player, 4));
                else
                    state.EntitiesInBattle = CreateMechs(Loyalty.EnemyArmy, 4).Union(CreatePlayerMechs(GlobalState.StrategicMap.State.EquippedEntities));
                GlobalState.TacticalBattle = state;
            }
        }

        // create a collection of mechs
        private IEnumerable<ActiveEntity> CreateMechs(Loyalty loyalty, int number)
        {
            var entityTemplates = GlobalState.Configurations.EntityTemplates.GetAllConfigurations();
            return Enumerable.Range(0, number).Select(num =>
                {
                    var template = entityTemplates.ChooseRandomValue();
                    return (ActiveEntity)new MovingEntity(
                   new SpecificEntity(template),
                   loyalty,
                   ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>(),
                   GlobalState.Configurations.SubsystemTemplates.GetAllConfigurations().
                       ChooseRandomValues(template.SystemSlots).Select(systemTemplate => new Subsystem(systemTemplate, loyalty))

                   );
                }).Materialize();
        }

        // create the player controlled mechs from their definition in the global state
        private IEnumerable<ActiveEntity> CreatePlayerMechs(List<EquippedEntity> equippedEntities)
        {
            return equippedEntities.Select(entity => (ActiveEntity)new MovingEntity(
                entity.Entity,
                Loyalty.Player,
                ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>(),
                entity.Subsystems.Where(template => template != null).Select(template => new Subsystem(template, Loyalty.Player)))).Materialize();
        }

        #endregion private methods
    }
}