using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Small handoff for Title -> Main

public static class GameBoot
{
    public enum Mode
    {
        AutoLoad,
        NewGame,
        LoadGame
    }


    public static Mode RequestedMode = Mode.AutoLoad;

    //Optional

    public static void ResetToAuto()
    {
        RequestedMode = Mode.AutoLoad;
    }

    //Optional: for tests

    public static void ForceNew()
    {
        RequestedMode = Mode.NewGame;
    }
}