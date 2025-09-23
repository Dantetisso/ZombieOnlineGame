using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomLeaver : MonoBehaviourPunCallbacks
{
    public static RoomLeaver Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LeaveRoom()
    {
        Debug.Log("<color=cyan> Sali de la room </color>");
        Cursor.visible = true;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneLoader.LoadScene(ScenesEnum.MainMenu);
    }
}
