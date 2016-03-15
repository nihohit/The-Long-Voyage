namespace Assets.Scripts.InterSceneCommunication
{
    using Assets.Scripts.Base;
    using Assets.Scripts.Base.JsonParsing;
    using Assets.Scripts.LogicBase;
    using Assets.Scripts.StrategicGameScene;

    #region Configurations

    public class Configurations
    {
        #region fields

        private ConfigurationStorage<EntityTemplate> m_activeEntities;

        private ConfigurationStorage<TerrainEntityTemplate> m_terrainEntities;

        private ConfigurationStorage<HexEffectTemplate> m_hexEffects;

        private ConfigurationStorage<SubsystemTemplate> m_subsystems;

        private ConfigurationStorage<VignetteTemplate> m_vignettes;

        private ConfigurationStorage<StartingScenarioTemplate> m_scenarios;

        #endregion fields

        #region properties

        public ConfigurationStorage<EntityTemplate> ActiveEntities
        {
            get
            {
                if (m_activeEntities == null)
                {
                    m_activeEntities = new ConfigurationStorage<EntityTemplate>("MovingEntities");
                }

                return m_activeEntities;
            }
        }

        public ConfigurationStorage<TerrainEntityTemplate> TerrainEntities
        {
            get
            {
                if (m_terrainEntities == null)
                {
                    m_terrainEntities = new ConfigurationStorage<TerrainEntityTemplate>("TerrainEntities");
                }

                return m_terrainEntities;
            }
        }

        public ConfigurationStorage<HexEffectTemplate> HexEffects
        {
            get
            {
                if (m_hexEffects == null)
                {
                    m_hexEffects = new ConfigurationStorage<HexEffectTemplate>("HexEffects");
                }

                return m_hexEffects;
            }
        }

        public ConfigurationStorage<SubsystemTemplate> Subsystems
        {
            get
            {
                if (m_subsystems == null)
                {
                    m_subsystems = new ConfigurationStorage<SubsystemTemplate>("Subsystems");
                }

                return m_subsystems;
            }
        }

        public ConfigurationStorage<VignetteTemplate> Vignettes
        {
            get
            {
                if (m_vignettes == null)
                {
                    m_vignettes = new ConfigurationStorage<VignetteTemplate>("Vignettes");
                }

                return m_vignettes;
            }
        }

        public ConfigurationStorage<StartingScenarioTemplate> StartingScenarios
        {
            get
            {
                if (m_scenarios == null)
                {
                    m_scenarios = new ConfigurationStorage<StartingScenarioTemplate>("StartingScenarios");
                }

                return m_scenarios;
            }
        }

        #endregion properties
    }

    #endregion Configurations
}