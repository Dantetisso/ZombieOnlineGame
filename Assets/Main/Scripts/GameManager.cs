using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static event Action<int> OnAlivePlayersChanged;

    private int deadPlayers = 0;

    // va contando jugadores q murieron
    private readonly HashSet<int> deadPlayersIDs = new();

    public override void OnEnable()
    {
        base.OnEnable();
        HealthScript.OnPlayerDied += HandlePlayerDied;
    }

    public override  void OnDisable()
    {
        base.OnDisable();
        HealthScript.OnPlayerDied -= HandlePlayerDied;
    }

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            int alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
            OnAlivePlayersChanged?.Invoke(alivePlayers);
        }
    }

    private void HandlePlayerDied(Player player)
    {
        // Evita contar al mismo player
        if (deadPlayersIDs.Contains(player.ActorNumber)) return;

        deadPlayersIDs.Add(player.ActorNumber);
        deadPlayers++;

        int alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
        OnAlivePlayersChanged?.Invoke(alivePlayers);

        Debug.Log($"{player.NickName} died. Alive players: {alivePlayers}");

        if (PhotonNetwork.IsMasterClient && alivePlayers <= 0)
        {
            PhotonNetwork.LoadLevel(ScenesEnum.GameOver.ToString());
        }
    }
}
