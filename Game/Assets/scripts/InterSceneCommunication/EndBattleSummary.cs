using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.InterSceneCommunication
{
    /// <summary>
    /// The results of a battle, regarding end of battle reeport and effect on strategic game
    /// </summary>
    public class EndBattleSummary
    {
        // Player's units which survived the battle, in the state they survived
        public IEnumerable<EquippedEntity> SurvivingEntities { get; private set; }

        // Destroyed entities which were salvaged & repaired
        public IEnumerable<SpecificEntity> SalvagedEntities { get; private set; }

        // Systems which were salvaged from destroyed entities
        public IEnumerable<SubsystemTemplate> SalvagedSystems { get; private set; }

        public EndBattleSummary(
            IEnumerable<EquippedEntity> survivingEntities,
            IEnumerable<SpecificEntity> salvagedEntities,
            IEnumerable<SubsystemTemplate> salvagedEquipement)
        {
            SurvivingEntities = survivingEntities.Materialize();
            SalvagedEntities = salvagedEntities.Materialize();
            SalvagedSystems = salvagedEquipement.Materialize();
            Debug.Log("Entities survived: {0}".FormatWith(SurvivingEntities.ToJoinedString()));
            Debug.Log("Entities were salvaged: {0}".FormatWith(SalvagedEntities.ToJoinedString()));
            Debug.Log("Systems were salvaged: {0}".FormatWith(SalvagedSystems.Select(system => system.Name).ToJoinedString()));
        }
    }
}