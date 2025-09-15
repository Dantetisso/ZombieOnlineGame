using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static event Action<int> OnAlivePlayersChanged;

    private int alivePlayers;
    private int deadPlayers = 0;

    // para contar muertos
    private readonly System.Collections.Generic.HashSet<int> deadPlayersIDs = new();

    public override void OnEnable()
    {
        HealthScript.OnPlayerDied += HandlePlayerDied;
    }

    public override void OnDisable()
    {
        HealthScript.OnPlayerDied -= HandlePlayerDied;
    }

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            OnAlivePlayersChanged?.Invoke(alivePlayers);
        }
    }

    private void HandlePlayerDied(Player player)
    {
        // se registra una vez x jugador
        if (!deadPlayersIDs.Contains(player.ActorNumber))
        {
            deadPlayersIDs.Add(player.ActorNumber);
            deadPlayers++;

            int currentAlive = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
            OnAlivePlayersChanged?.Invoke(currentAlive);

            Debug.Log($"{player.NickName}: Died  - Alive: {currentAlive}");

            // el master carga la escena de gameover
            if (PhotonNetwork.IsMasterClient && currentAlive <= 0)
            {
                PhotonNetwork.LoadLevel(ScenesEnum.GameOver.ToString());
            }
        }
    }
}
