using Assets.Scripts.LogicBase;
using System.Collections.Generic;

namespace Assets.Scripts.InterSceneCommunication
{
    /// <summary>
    /// represents the state of player-controlled forces on the strategic gameplay
    /// </summary>
    public class PlayerState
    {
        public string Name { get; private set; }

        // Systems that aren't slotted in an equipped entity
        public List<SubsystemTemplate> AvailableSystems { get; private set; }

        // Unequipped entities, available for usge
        public List<SpecificEntity> AvailableEntities { get; private set; }

        // Entities with set equipment - those will be used in battle
        public List<EquippedEntity> EquippedEntities { get; private set; }

        public PlayerState()
        {
            AvailableSystems = new List<SubsystemTemplate>();
            AvailableEntities = new List<SpecificEntity>();
            EquippedEntities = new List<EquippedEntity>();
        }
    }
}