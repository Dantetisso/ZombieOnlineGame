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
    [SerializeField] Button OfflineButton;
    [SerializeField] Button quitButton;

    void Start()
    {
        playButton.onClick.AddListener(Play);
        OfflineButton.onClick.AddListener(PlaySolo);
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

    void quit()
    {
        Application.Quit();
    }
    
    void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
        if (OfflineButton != null)
            OfflineButton.onClick.RemoveAllListeners();
        if (quitButton != null)
            quitButton.onClick.RemoveAllListeners();
    }
}
