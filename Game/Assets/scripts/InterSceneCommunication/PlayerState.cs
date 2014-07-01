using Assets.scripts.LogicBase;
using System.Collections.Generic;

namespace Assets.scripts.InterSceneCommunication
{
    public class PlayerState
    {
        public string Name { get; private set; }
        public List<SubsystemTemplate> AvailableSystems { get; private set; }
        public List<SpecificEntity> AvailableEntities { get; private set; }
        public List<EquippedEntity> EquippedEntities { get; private set; }

        public PlayerState()
        {
            AvailableSystems = new List<SubsystemTemplate>();
            AvailableEntities = new List<SpecificEntity>();
            EquippedEntities = new List<EquippedEntity>();
        }
    }
}