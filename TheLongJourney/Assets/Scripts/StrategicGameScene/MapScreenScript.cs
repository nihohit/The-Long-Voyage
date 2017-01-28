using System.Collections;
using UnityEngine;

public class MapScreenScript : MonoBehaviour
{
    public Canvas LoadupScreen;

    public void SwitchToLoadupScreen()
    {
        LoadupScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}