﻿using System.Collections.Generic;
using Assets.scripts.TacticalBattleScene;

namespace Assets.scripts.InterSceneCommunication
{
    /// <summary>
    /// Information relevant for the beginning of a tactical battle.
    /// Mostly map generation information and which units participate
    /// </summary>
    public class TacticalBattleInformation
    {
        // Determines map size
        public int AmountOfHexes { get; set; }

        //TODO - should be converted to EquippedEntity
        public IEnumerable<ActiveEntity> EntitiesInBattle { get; set; }
    }
}