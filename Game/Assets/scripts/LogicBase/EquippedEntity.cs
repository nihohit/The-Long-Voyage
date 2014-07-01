using System;
using System.Collections.Generic;
using System.Linq;


namespace Assets.scripts.LogicBase
{
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
