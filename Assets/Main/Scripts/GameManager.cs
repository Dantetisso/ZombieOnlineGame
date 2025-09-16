using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Configuración del juego")]
    [SerializeField] private GameConfig gameConfig;

    private int currentWave;
    private int zombiesAlive;
    private int deadPlayers = 0;
    private bool isBossWave = false;

    public int CurrentWave => currentWave;
    public int ZombiesAlive => zombiesAlive;

    public static event Action<int> OnAlivePlayersChanged;
    public static event Action<int> OnZombiesAliveChanged;
    public static event Action<int, int, bool> OnWaveStarted; // wave, amount, bossWave
    public static event Action OnVictory;

    private readonly HashSet<int> deadPlayersIDs = new();

    #region Unity Callbacks
    void Start()
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig no asignado en GameManager!");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            int alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
            OnAlivePlayersChanged?.Invoke(alivePlayers);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartNextWave();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        HealthScript.OnPlayerDied += HandlePlayerDied;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        HealthScript.OnPlayerDied -= HandlePlayerDied;
    }
    #endregion

    #region Player Handling
    private void HandlePlayerDied(Player player)
    {
        if (deadPlayersIDs.Contains(player.ActorNumber)) return;

        deadPlayersIDs.Add(player.ActorNumber);
        deadPlayers++;

        int alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
        OnAlivePlayersChanged?.Invoke(alivePlayers);

        Debug.Log($"{player.NickName} died. Players alive: {alivePlayers}");

        if (PhotonNetwork.IsMasterClient && alivePlayers <= 0)
        {
            PhotonNetwork.LoadLevel(ScenesEnum.GameOver.ToString());
        }
    }
    #endregion

    #region Waves
    public void StartNextWave()
    {
        if (gameConfig == null) return;

        currentWave++;

        if (currentWave > gameConfig._maxWaves)
        {
            return;
        }

        isBossWave = gameConfig._bossRound > 0 && currentWave % gameConfig._bossRound == 0;
        int amount = isBossWave ? 1 : gameConfig._baseZombies + (currentWave - 1) * gameConfig._zombiesPerRound;

        zombiesAlive = amount;

        OnWaveStarted?.Invoke(currentWave, amount, isBossWave);
        OnZombiesAliveChanged?.Invoke(zombiesAlive);
    }

    public void OnZombieDied(bool wasBoss)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        zombiesAlive--;
        OnZombiesAliveChanged?.Invoke(zombiesAlive);

        if (isBossWave)
        {
            if (wasBoss)
            {
                Victory();
            }
        }
        else
        {
            if (zombiesAlive <= 0)
            {
                Victory();
                StartNextWave();
            }
        }
    }
    #endregion

    #region Victory
    private void Victory()
    {
        Debug.Log("¡win!");
        OnVictory?.Invoke();
        photonView.RPC(nameof(RPC_Victory), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Victory()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SceneLoader.LoadSceneByPhoton(ScenesEnum.Victory);
        }
    }
    #endregion
}
