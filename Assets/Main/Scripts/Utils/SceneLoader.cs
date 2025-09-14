using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public enum ScenesEnum // poner con nombre exacto de la escena
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

    public static void LoadSceneByPhoton(ScenesEnum scene)
    {
        PhotonNetwork.LoadLevel(scene.ToString());
    }
}
