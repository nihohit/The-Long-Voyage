using System;
namespace Assets.Scripts.StrategicGameScene
{
    [Flags]
    public enum ChoiceResults 
    { 
        None = 0, 
        Fight = 1, 
        LosePilot = 2, 
        LoseMech = 4, 
        GetLoot = 8, 
        GetPilot = 16, 
        GetMech = 32 
    }
}