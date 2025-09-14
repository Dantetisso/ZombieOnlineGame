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
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerNickNameTexts;
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        gameManager = FindObjectOfType<GameManager>();
        EmptyTexts();

        if (!photonView.IsMine) startGameButton.gameObject.SetActive(false); // para que el boton de play solo lo pueda ver el master 

        ConnectionManager.Instance.OnJoinRoom += UpdateTexts;
        ConnectionManager.Instance.OnPlayerEnterRoom += UpdateTexts;
        ConnectionManager.Instance.OnPlayerLeaveRoom += UpdateTexts;
        UpdateTexts();
    }

    void OnStartGameButtonClicked() // al apretar el boton empieza el juego
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameManager.StartGame();
        }

        Debug.Log("<color=green>Botón de play tocado</color>");
    }

    private void UpdateTexts()
    {
        for (int i = 0; i < playerNickNameTexts.Length; i++)
        {
            playerNickNameTexts[i].text = "";
        }

        // Obtengo jugadores conectados
        Dictionary<int, Player> players = ConnectionManager.Instance.GetPlayersInRoom();

        //ordeno los jugadores por actornumber (orden de entrada) 
        List<Player> orderedPlayers = new List<Player>(players.Values);
        orderedPlayers.Sort((p1, p2) => p1.ActorNumber.CompareTo(p2.ActorNumber));

        int index = 0;
        foreach (Player player in orderedPlayers)
        {
            if (index < playerNickNameTexts.Length)
            {
                playerNickNameTexts[index].text = player.NickName;
                index++;
            }
        }

        // Habilito el botón solo si hay entre 2 y 4 jugadores
        int count = orderedPlayers.Count;
        startGameButton.interactable = count >= 2 && count <= 4;

        Debug.Log($"[Lobby] Jugadores en la sala: {count}");
    }

    void EmptyTexts()
    {
        for (int i = 0; i < playerNickNameTexts.Length; i++)
        {
            playerNickNameTexts[i].text = "";
        }
    }

    void OnDestroy() // desuscribiendose para que cuando un jugador salga de la partida no siga llamando a los eventos y se rompa todo
    {
        if (ConnectionManager.Instance == null) return;

        ConnectionManager.Instance.OnJoinRoom        -= UpdateTexts;
        ConnectionManager.Instance.OnPlayerEnterRoom -= UpdateTexts;
        ConnectionManager.Instance.OnPlayerLeaveRoom -= UpdateTexts;
    }
    
}
