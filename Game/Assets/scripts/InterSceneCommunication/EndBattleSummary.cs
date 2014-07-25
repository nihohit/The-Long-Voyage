using System;
using System.Collections.Generic;
using System.Linq;
using Assets.scripts.LogicBase;

namespace Assets.scripts.InterSceneCommunication
{
    public class EndBattleSummary
    {
        public IEnumerable<EquippedEntity> SurvivingEntities { get; private set; }
        public IEnumerable<SpecificEntity> SalvagedEntities { get; private set; }
        public IEnumerable<SubsystemTemplate> SalvagedSystems { get; private set; }
        public EndBattleSummary(IEnumerable<EquippedEntity> survivingEntities, 
                                IEnumerable<SpecificEntity> salvagedEntities, 
                                IEnumerable<SubsystemTemplate> salvagedEquipement)
        {
            SurvivingEntities = survivingEntities;
            SalvagedEntities = salvagedEntities;
            SalvagedSystems = salvagedEquipement;
        }
    }
}
