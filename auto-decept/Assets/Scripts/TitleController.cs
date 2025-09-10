using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


//Title buttons: New Game / Load Game
//If no save exists, show message and fall back to New Game
public class TitleController : MonoBehaviour
{
    [Header("Optional")]
    public TextMeshProUGUI statusText;

    public void OnNewGame()
    {
        GameBoot.RequestedMode = GameBoot.Mode.NewGame;
        SceneManager.LoadScene("Main");

    }

    public void OnLoadGame()
    {
        SaveData tmp;
        if (!SaveSystem.TryLoad(out tmp))
        {
            if (statusText != null)
            {
                statusText.text = "No save file found. Starting a New Game...";
            }
            GameBoot.RequestedMode = GameBoot.Mode.NewGame;
        }
        else
        {
            GameBoot.RequestedMode = GameBoot.Mode.LoadGame;
        }

        SceneManager.LoadScene("Main");
    }
}
