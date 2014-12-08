using Assets.Scripts.StrategicGameScene;

namespace Assets.Scripts.InterSceneCommunication
{
    /// <summary>
    /// Information relevant to the strategic gameplay, to be passed from one encounter to the next.
    /// </summary>
    public class StrategicMapInformation
    {
        public PlayerState State { get; set; }

        public Location CurrentLocation { get; set; }
    }
}