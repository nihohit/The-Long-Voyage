using Assets.Scripts.LogicBase;

namespace Assets.Scripts.InterSceneCommunication
{
    #region Configurations

    public class Configurations
    {
        #region properties

        public EntityTemplateStorage EntityTemplates { get; private set; }

        public SubsystemTemplateStorage SubsystemTemplates { get; private set; }

        #endregion properties

        #region constructors

        public Configurations()
        {
            EntityTemplates = new EntityTemplateStorage("MovingEntities");
            SubsystemTemplates = new SubsystemTemplateStorage("Subsystems");
        }

        #endregion constructors
    }

    #endregion Configurations
}