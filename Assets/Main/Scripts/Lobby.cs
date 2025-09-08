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
        if (!photonView.IsMine) createRoomButton.gameObject.SetActive(false);

        ConnectionManager.Instance.OnJoinRoom += UpdateTexts;
        ConnectionManager.Instance.OnPlayerEnterRoom += UpdateTexts;
        ConnectionManager.Instance.OnPlayerLeaveRoom += UpdateTexts;
        UpdateTexts();
    }

    void OnCreateRoomButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            manager.StartGame();
        }

        Debug.Log("<color=green>Botón tocado</color>");
    }

    private void UpdateTexts()
    {
        // Limpio todos los textos primero
        for (int i = 0; i < playerNickNameTexts.Length; i++)
        {
            playerNickNameTexts[i].text = "";
        }

        // Obtengo jugadores conectados
        Dictionary<int, Player> players = ConnectionManager.Instance.GetPlayersInRoom();

        int index = 0;
        foreach (KeyValuePair<int, Player> player in players)
        {
            if (index < playerNickNameTexts.Length) // seguridad por si hay más jugadores que slots
            {
                playerNickNameTexts[index].text = player.Value.NickName;
                index++;
            }
        }

        // Habilito el botón solo si hay entre 2 y 4 jugadores
        int count = players.Count;
        createRoomButton.interactable = count >= 2 && count <= 4;

        Debug.Log($"[Lobby] Jugadores en la sala: {count}");
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

        ConnectionManager.Instance.OnJoinRoom        -= UpdateTexts;
        ConnectionManager.Instance.OnPlayerEnterRoom -= UpdateTexts;
        ConnectionManager.Instance.OnPlayerLeaveRoom -= UpdateTexts;
    }
    
}
