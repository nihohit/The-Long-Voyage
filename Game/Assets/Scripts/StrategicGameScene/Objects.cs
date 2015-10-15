﻿using System;

namespace Assets.Scripts.StrategicGameScene
{
    [Flags]
    public enum ChoiceResultType
    {
        None = 0,
        Fight = 1,
        AffectRelations = 2,
    }

    public enum ConditionType
    {
        None,
        RelationsWith,
    }


    public enum Biome { Jungle, Mountain, Forest, Hills, Undefined, Desert, Sea, Plains }
}