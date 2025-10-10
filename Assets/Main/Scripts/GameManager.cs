using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Configuración del juego")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private LevelUIController lvlUI;

    private int maxWaves;
    private int baseZombies;
    private int zombiesPerRound;
    private int bossRound;
    private int currentWave;
    private int zombiesAlive;
    private int deadPlayers = 0;
    private bool isBossWave = false;

    public int CurrentWave => currentWave;
    public int ZombiesAlive => zombiesAlive;

    public static event Action<int> OnAlivePlayersChanged;
    public static event Action<int> OnZombiesAliveChanged;
    public static event Action<int, int, bool> OnWaveStarted;
    public static event Action OnVictory;

    private readonly HashSet<int> deadPlayersIDs = new();

    private void Start()
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig no asignado en GameManager!");
            return;
        }

        ReadConfig();

        if (PhotonNetwork.InRoom)
        {
            int alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
            OnAlivePlayersChanged?.Invoke(alivePlayers);
        }

        if (PhotonNetwork.IsMasterClient)
            StartNextWave();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PlayerMovement.OnPlayerDied += HandlePlayerDied;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayerMovement.OnPlayerDied -= HandlePlayerDied;
    }

    private void ReadConfig()
    {
        maxWaves = gameConfig._maxWaves;
        baseZombies = gameConfig._baseZombies;
        zombiesPerRound = gameConfig._zombiesPerRound;
        bossRound = gameConfig._bossRound;
    }

    public void StartNextWave()
    {
        currentWave++;

        if (currentWave > maxWaves)
        {
            Victory();
            return;
        }

        isBossWave = bossRound > 0 && currentWave % bossRound == 0;

        // Cantidad de enemigos según si es boss o ronda normal
        int amount = isBossWave ? 1 : baseZombies + (currentWave - 1) * zombiesPerRound;
        zombiesAlive = amount;

        // MasterClient actualiza UI
        OnWaveStarted?.Invoke(currentWave, amount, isBossWave);
        OnZombiesAliveChanged?.Invoke(zombiesAlive);

        // Sincronizar con todos los clientes
        photonView.RPC(nameof(RPC_UpdateWaveInfo), RpcTarget.Others, currentWave, amount, isBossWave);
        photonView.RPC(nameof(RPC_UpdateZombiesAlive), RpcTarget.Others, zombiesAlive);
        photonView.RPC(nameof(RPC_ActivateBossWarning), RpcTarget.All, isBossWave);  // Activar el GameObject en rondas de boss
    }

    public void OnZombieDied(bool wasBoss)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        zombiesAlive--;
        OnZombiesAliveChanged?.Invoke(zombiesAlive);

        photonView.RPC(nameof(RPC_UpdateZombiesAlive), RpcTarget.Others, zombiesAlive);

        if (isBossWave)
        {
            if (wasBoss)
                Victory();
        }
        else
        {
            if (zombiesAlive <= 0)
                StartNextWave();
        }
    }

    private void HandlePlayerDied(int viewID)
    {
        if (deadPlayersIDs.Contains(viewID)) return;

        deadPlayersIDs.Add(viewID);
        deadPlayers++;

        int alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
        OnAlivePlayersChanged?.Invoke(alivePlayers);

        if (PhotonNetwork.IsMasterClient && alivePlayers <= 0)
            PhotonNetwork.LoadLevel((nameof(ScenesEnum.GameOver)));
    }

    private void Victory()
    {
        OnVictory?.Invoke();
        photonView.RPC(nameof(RPC_Victory), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Victory()
    {
        if (PhotonNetwork.IsMasterClient) SceneLoader.LoadSceneByPhoton(ScenesEnum.Victory);
    }

    [PunRPC]
    private void RPC_UpdateWaveInfo(int wave, int amount, bool bossWave)
    {
        OnWaveStarted?.Invoke(wave, amount, bossWave);
    }

    [PunRPC]
    private void RPC_UpdateZombiesAlive(int alive)
    {
        OnZombiesAliveChanged?.Invoke(alive);
    }

    [PunRPC]
    private void RPC_ActivateBossWarning(bool isBossWave)
    {
        // sincroniza el mensaje de warning del boss
        lvlUI.SetBossWarningActive(isBossWave);
    }
}
