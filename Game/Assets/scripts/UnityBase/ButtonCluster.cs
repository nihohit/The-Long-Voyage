using System.Collections.Generic;
using Assets.scripts.Base;

namespace Assets.scripts.UnityBase
{
    /// <summary>
    /// Used when a group of buttons need to be operated on simultaneously.
    /// </summary>
    public class ButtonCluster
    {
        private readonly IEnumerable<IUnityButton> m_buttons;

        public ButtonCluster(IEnumerable<IUnityButton> buttons)
        {
            m_buttons = buttons;
            foreach (var button in m_buttons)
            {
                var currentTask = button.ClickableAction;
                button.ClickableAction = () =>
                    {
                        currentTask();
                        DestroyCluster();
                    };
            }
        }

        // destroy all the buttons in the dluster.
        public void DestroyCluster()
        {
            m_buttons.ForEach(button => button.DestroyGameObject());
        }
    }
}