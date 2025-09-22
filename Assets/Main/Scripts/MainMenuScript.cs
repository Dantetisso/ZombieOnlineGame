using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuScript : MonoBehaviourPunCallbacks
{
    [SerializeField] Button playButton;
    [SerializeField] Button soloButton;
    [SerializeField] Button quitButton;

    void Start()
    {
        playButton.onClick.AddListener(Play);
        soloButton.onClick.AddListener(playo);
        quitButton.onClick.AddListener(quit);
    }

    void Play()
    {
        SceneLoader.LoadScene(ScenesEnum.RoomList);
    }

    void PlaySolo()
    {
        PhotonNetwork.OfflineMode = true; // modo offline
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 1 });
        SceneLoader.LoadSceneByPhoton(ScenesEnum.Level);
    }

    void playo()
    {
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions() { MaxPlayers = 1 }, default);
        SceneLoader.LoadSceneByPhoton(ScenesEnum.Level);
    }

    void quit()
    {
        Application.Quit();
    }
    
    void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
        if (soloButton != null)
            soloButton.onClick.RemoveAllListeners();
        if (quitButton != null)
            quitButton.onClick.RemoveAllListeners();
    }
}
