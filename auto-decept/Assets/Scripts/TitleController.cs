using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


//Title buttons: New Game / Load Game
//If no save exists, show message and fall back to New Game
public class TitleController : MonoBehaviour
{
    public TextMeshProUGUI statusText;

    public void OnNewGame()
    {
        GameBoot.RequestedMode = GameBoot.Mode.NewGame;
        SceneManager.LoadScene("Main");

    }

    public void OnLoadGame()
    {
        GameBoot.RequestedMode = GameBoot.Mode.LoadGame;
        if (statusText != null) statusText.text = "Loading saved game...";
        SceneManager.LoadScene("Main");
    }
}
