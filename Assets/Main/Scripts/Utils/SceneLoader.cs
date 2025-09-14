using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ScenesEnum // poner con nombre exacto
{
    MainMenu,
    RoomList,
    Lobby,
    Level,
    Victory,
    GameOver,
}

public static class SceneLoader 
{
    public static void LoadScene(ScenesEnum scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public static AsyncOperation LoadSceneAsync(ScenesEnum scene)
    {
        return SceneManager.LoadSceneAsync(scene.ToString());
    }
}
