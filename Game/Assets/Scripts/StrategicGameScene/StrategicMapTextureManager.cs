using UnityEngine;

namespace Assets.Scripts.StrategicGameScene
{
    internal class StrategicMapTextureHandler
    {
        private readonly Texture r_guiBackground;

        public StrategicMapTextureHandler()
        {
            this.r_guiBackground = Resources.Load<Texture2D>("StrategicMapUI/LocationMessageBackground");
        }

        internal UnityEngine.Texture GetUIBackground()
        {
            return this.r_guiBackground;
        }
    }
}