using System.Collections.Generic;

namespace Assets.Scripts.LogicBase
{
    using Assets.Scripts.Base;
    using Assets.Scripts.Base.JsonParsing;
    using Assets.Scripts.InterSceneCommunication;
    using System.Linq;

    /// <summary>
    /// defines the full template for an entity, including all defining variables.
    /// From this the tactical representation of an entity can be created.
    /// </summary>
    public class EquippedEntity
    {
        public SpecificEntity InternalEntity { get; private set; }

        public IEnumerable<SubsystemTemplate> Subsystems { get; private set; }

        [ChosenConstructorForParsing]
        public EquippedEntity(SpecificEntity entity, IEnumerable<string> subsystems) :
            this(entity, subsystems.Select(name => GlobalState.Instance.Configurations.Subsystems.GetConfiguration(name)).ToList())
        {
        }

        public EquippedEntity(SpecificEntity entity, IEnumerable<SubsystemTemplate> subsystems)
        {
            InternalEntity = entity;
            Subsystems = subsystems;
        }

		public override string ToString()
		{
			return InternalEntity.ToString(); 
		}
	}
}