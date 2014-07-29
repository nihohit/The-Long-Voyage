using System.Collections.Generic;

namespace Assets.scripts.LogicBase
{
    /// <summary>
    /// defines the full template for an entity, including all defining variables.
    /// From this the tactical representation of an entity can be created.
    /// </summary>
    public class EquippedEntity
    {
        public SpecificEntity Entity { get; private set; }

        public IEnumerable<SubsystemTemplate> Subsystems { get; private set; }

        public EquippedEntity(SpecificEntity entity, IEnumerable<SubsystemTemplate> subsystems)
        {
            Entity = entity;
            Subsystems = subsystems;
        }
    }
}