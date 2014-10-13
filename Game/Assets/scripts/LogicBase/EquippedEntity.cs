﻿using System.Collections.Generic;

namespace Assets.Scripts.LogicBase
{
    /// <summary>
    /// defines the full template for an entity, including all defining variables.
    /// From this the tactical representation of an entity can be created.
    /// </summary>
    public class EquippedEntity
    {
        public SpecificEntity InternalEntity { get; private set; }

        public IEnumerable<SubsystemTemplate> Subsystems { get; private set; }

        public EquippedEntity(SpecificEntity entity, IEnumerable<SubsystemTemplate> subsystems)
        {
            InternalEntity = entity;
            Subsystems = subsystems;
        }
    }
}