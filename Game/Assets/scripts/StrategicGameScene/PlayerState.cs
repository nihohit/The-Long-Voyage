using Assets.scripts.LogicBase;
using System.Collections.Generic;

namespace Assets.scripts.StrategicGameScene
{
    public class PlayerState
    {
        private string m_name;
        private List<SubsystemTemplate> m_availableSystems = new List<SubsystemTemplate>();
        private List<EntityTemplate> m_availableEntities = new List<EntityTemplate>();
        private List<EquippedEntity> m_equippedEntities = new List<EquippedEntity>();
        private List<EquippedEntity> m_entitiesInRepair = new List<EquippedEntity>();
    }
}