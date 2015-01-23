using UnityEngine;

namespace Assets.Scripts.StrategicGameScene
{
    internal class StrategicMapTextureHandler
    {
        private readonly Texture r_GUIBackground;

        public StrategicMapTextureHandler()
        {
            r_GUIBackground = Resources.Load<Texture2D>("StrategicMapUI/LocationMessageBackground");
        }

        internal UnityEngine.Texture GetUIBackground()
        {
            return r_GUIBackground;
        }
    }
}