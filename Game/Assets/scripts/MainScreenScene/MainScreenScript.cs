using System;
using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using UnityEngine;

namespace Assets.Scripts.MainScreenScene
{
    public class MainScreenScript : MonoBehaviour
    {
        private float m_screenWidth, m_screenHeight, m_sliderHeight, m_sliderWidth, m_buttonHeight, m_markerHeight;

        private int m_sliderValue, m_minHexSlider, m_maxHexSlider;

        private void Start()
        {
            m_minHexSlider = SimpleConfigurationHandler.GetIntProperty("min number of hexes", FileAccessor.General);
            m_maxHexSlider = SimpleConfigurationHandler.GetIntProperty("max number of hexes", FileAccessor.General);
            m_sliderValue = (m_minHexSlider + m_maxHexSlider) / 3;
            m_screenWidth = Screen.width;
            m_screenHeight = Screen.height;
            m_sliderHeight = m_screenHeight * 0.3f;
            m_buttonHeight = m_screenHeight * 0.6f;
            m_sliderWidth = m_screenWidth / 2;
            m_markerHeight = m_screenHeight * 0.4f;
        }

        private void OnGUI()
        {
            DisplayButtons();
        }

        private void DisplayButtons()
        {
            m_sliderValue = Convert.ToInt32(GUI.HorizontalSlider(new Rect(m_sliderWidth - 40, m_sliderHeight, 80, 20), m_sliderValue,
                                                               m_minHexSlider, m_maxHexSlider));
            GUI.Box(new Rect(m_sliderWidth - 40, m_markerHeight, 80, 20), m_sliderValue + " hexes");

            if (GUI.Button(new Rect(m_sliderWidth - 40, m_buttonHeight, 80, 20), "StartGame"))
            {
                GlobalState.TacticalBattle.AmountOfHexes = m_sliderValue;
                Application.LoadLevel("TacticalBattleScene");
            }
        }
    }
}