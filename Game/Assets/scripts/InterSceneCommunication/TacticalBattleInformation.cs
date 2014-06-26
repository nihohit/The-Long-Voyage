﻿using Assets.scripts.TacticalBattleScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.scripts.InterSceneCommunication
{
    public class TacticalBattleInformation
    {
        public int AmountOfHexes { get; set; }

        public IEnumerable<ActiveEntity> EntitiesInBattle { get; set; }
    }
}