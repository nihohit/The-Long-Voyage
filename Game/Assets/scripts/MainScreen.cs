using UnityEngine;
using System.Collections;
using System;

public class MainScreen : MonoBehaviour 
{
	private float screenWidth, screenHeight, sliderHeight, sliderWidth,  buttonHeight, markerHeight;

    private int sliderValue = 1;

	void Start()
	{
		screenWidth = Screen.width;	
		screenHeight = Screen.height;
		sliderHeight = screenHeight * 0.3f;
        buttonHeight = screenHeight * 0.6f;
		sliderWidth = screenWidth / 2;
        markerHeight = screenHeight * 0.4f;
	}
	
	void OnGUI()
	{
		DisplayButtons();
	}

	void DisplayButtons()
	{
        sliderValue = Convert.ToInt32(GUI.HorizontalSlider(new Rect(sliderWidth - 40, sliderHeight, 80, 20), sliderValue, 1, 10));
        GUI.Box(new Rect(sliderWidth - 40, markerHeight, 80, 20), sliderValue + " hexes");

        if(GUI.Button(new Rect(sliderWidth - 40, buttonHeight, 80, 20), "StartGame"))
        {
            GlobalState.Instance.AmountOfHexes = sliderValue;
            Application.LoadLevel("generateLevel");
        }

		
	}
}
