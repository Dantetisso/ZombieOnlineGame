using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks  
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerNickNameTexts;

    void Start()
    {
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        EmptyTexts();

        if (!photonView.IsMine) startGameButton.gameObject.SetActive(false);

        if (ConnectionManager.Instance != null)
        {
            ConnectionManager.Instance.OnJoinRoom += SafeUpdateTexts;
            ConnectionManager.Instance.OnPlayerEnterRoom += SafeUpdateTexts;
            ConnectionManager.Instance.OnPlayerLeaveRoom += SafeUpdateTexts;
        }

        UpdateTexts();
    }

    void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SceneLoader.LoadScene(ScenesEnum.Level); 
        }
    }

    private void SafeUpdateTexts()
    {
        if (this == null || gameObject == null) return;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        for (int i = 0; i < playerNickNameTexts.Length; i++)
            playerNickNameTexts[i].text = "";

        Dictionary<int, Player> players = ConnectionManager.Instance?.GetPlayersInRoom() ?? new Dictionary<int, Player>();
        List<Player> orderedPlayers = new List<Player>(players.Values);
        orderedPlayers.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

        int index = 0;
        foreach (Player player in orderedPlayers)
        {
            if (index < playerNickNameTexts.Length)
            {
                playerNickNameTexts[index].text = player.NickName;
                index++;
            }
        }

        startGameButton.interactable = orderedPlayers.Count >= 2 && orderedPlayers.Count <= 4;

        Debug.Log($"[Lobby] Jugadores en la sala: {orderedPlayers.Count}");
    }

    void EmptyTexts()
    {
        for (int i = 0; i < playerNickNameTexts.Length; i++)
            playerNickNameTexts[i].text = "";
    }

    void OnDestroy()
    {
        if (ConnectionManager.Instance == null) return;

        ConnectionManager.Instance.OnJoinRoom        -= SafeUpdateTexts;
        ConnectionManager.Instance.OnPlayerEnterRoom -= SafeUpdateTexts;
        ConnectionManager.Instance.OnPlayerLeaveRoom -= SafeUpdateTexts;
    }
}
