using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Allows for scene management
using UnityEngine.SceneManagement;

public class Settings
{
    // Controls difficulty level
    public static int difficulty = 1;
}

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void PlayCampaign()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void PlayHorde()
    {
        SceneManager.LoadScene("Horde Mode");
    }

    public void QuitGame()
    {
        // Exit the game
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
