using System;
using System.Collections.Generic;
using System.Linq;


namespace Assets.scripts.LogicBase
{
    public class EquippedEntity
    {
        public EntityTemplate Entity { get; private set; }
        public IEnumerable<SubsystemTemplate> Subsystems { get; private set; }

        public EquippedEntity(EntityTemplate entity, IEnumerable<SubsystemTemplate> subsystems)
        {
            Entity = entity;
            Subsystems = subsystems;
        }
    }
}
