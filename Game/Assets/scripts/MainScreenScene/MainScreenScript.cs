﻿using Assets.scripts.Base;
using Assets.scripts.InterSceneCommunication;
using Assets.scripts.LogicBase;
using System;
using UnityEngine;

namespace Assets.scripts.MainScreenScene
{
    public class MainScreenScript : MonoBehaviour
    {
        private float screenWidth, screenHeight, sliderHeight, sliderWidth, buttonHeight, markerHeight;

        private int sliderValue, minHexSlider, maxHexSlider;

        private void Start()
        {
            FileHandler.Init();
            minHexSlider = FileHandler.GetIntProperty("min number of hexes", FileAccessor.General);
            maxHexSlider = FileHandler.GetIntProperty("max number of hexes", FileAccessor.General);
            sliderValue = (minHexSlider + maxHexSlider) / 3;
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            sliderHeight = screenHeight * 0.3f;
            buttonHeight = screenHeight * 0.6f;
            sliderWidth = screenWidth / 2;
            markerHeight = screenHeight * 0.4f;
        }

        private void OnGUI()
        {
            DisplayButtons();
        }

        private void DisplayButtons()
        {
            sliderValue = Convert.ToInt32(GUI.HorizontalSlider(new Rect(sliderWidth - 40, sliderHeight, 80, 20), sliderValue,
                                                               minHexSlider, maxHexSlider));
            GUI.Box(new Rect(sliderWidth - 40, markerHeight, 80, 20), sliderValue + " hexes");

            if (GUI.Button(new Rect(sliderWidth - 40, buttonHeight, 80, 20), "StartGame"))
            {
                GlobalState.TacticalBattle.AmountOfHexes = sliderValue;
                Application.LoadLevel("TacticalBattleScene");
            }
        }
    }
}