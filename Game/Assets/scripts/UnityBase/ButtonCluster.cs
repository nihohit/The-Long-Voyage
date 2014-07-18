using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.scripts.UnityBase
{
    public class ButtonCluster
    {
        private readonly IEnumerable<SimpleButton> m_buttons;

        public ButtonCluster(IEnumerable<SimpleButton> buttons)
        {
            m_buttons = buttons;
            foreach(var button in m_buttons)
            {
                var currentTask = button.ClickableAction;
                button.ClickableAction = () =>
                    {
                        currentTask();
                        DestroyCluster();
                    };
            }
        }

        public void DestroyCluster()
        { 
            foreach(var button in m_buttons)
            {
                button.DestroyGameObject();
            }
        }
    }
}
