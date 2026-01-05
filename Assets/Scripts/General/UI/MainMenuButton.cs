using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    public GameObject player;

    private bool paused;

    public void GoToMainMenu()
    {
        if(PauseManager.Instance != null)
        {
            PauseManager.Instance.Unpause();
        }
        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene("MainMenu");
        }
    }
}
