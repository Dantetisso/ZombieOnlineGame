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
        soloButton.onClick.AddListener(PlaySolo);
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
}
