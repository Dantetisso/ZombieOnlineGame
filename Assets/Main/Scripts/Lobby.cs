using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks  
{                                               
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_Text[] playerNickNameTexts;
    [SerializeField] private GameManager manager;

    void Start()
    {
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        manager = FindObjectOfType<GameManager>();
        EmptyTexts();

        ConnectionManager.Instance.OnJoinRoom += UpdatePlayers;
        ConnectionManager.Instance.OnPlayerEnterRoom += UpdatePlayers;
        ConnectionManager.Instance.OnPlayerLeaveRoom += UpdatePlayers;
    }

    void OnCreateRoomButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            manager.StartGame();
        }

        Debug.Log("<color=green>Botón tocado</color>");
    }

    private void UpdatePlayers()
    {
        Dictionary<int, Player> players = ConnectionManager.Instance.GetPlayersInRoom();
        Debug.Log("Players count" + players.Count);
        int index = 0;

        foreach (KeyValuePair<int, Player> player in players) // x cada jugador pongo el texto
        {
            playerNickNameTexts[index].text = player.Value.NickName;
            index++;
        }

        int count = ConnectionManager.Instance.GetPlayersInRoom().Count;
        createRoomButton.interactable = count >= 2 && count <= 4;

    }

    void EmptyTexts()
    {
        for (int i = 0; i < playerNickNameTexts.Length; i++)
        {
            playerNickNameTexts[i].text = "";
        }
    }

    void OnDestroy() // desuscribiendose para que cuando un jugador salga de la partida no siga llamando a los eventos y re rompa todo
    {
        if (ConnectionManager.Instance == null) return;

        ConnectionManager.Instance.OnJoinRoom        -= UpdatePlayers;
        ConnectionManager.Instance.OnPlayerEnterRoom -= UpdatePlayers;
        ConnectionManager.Instance.OnPlayerLeaveRoom -= UpdatePlayers;
    }
    
}
