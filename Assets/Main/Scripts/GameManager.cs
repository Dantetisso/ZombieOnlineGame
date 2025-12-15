using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
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
    private int alivePlayers;
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
            alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers;
            OnAlivePlayersChanged?.Invoke(alivePlayers);
        }

        // Solo iniciar nueva ronda si es el primer Master
        if (PhotonNetwork.IsMasterClient)
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentWave")) // chequeo en las propiedades de la room si ya hay un estado de ronda guardado
            {
                // cuando se crea la sala se guarda los jugadores muertos por pimera vez
                Hashtable props = new Hashtable { { "alivePlayers", PhotonNetwork.CurrentRoom.PlayerCount }};   // el 0 porque no murio nadie todavia
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                StartNextWave();
            }   // si no lo hay lo guardo
            else
            {
                // guardo 
                currentWave = (int)PhotonNetwork.CurrentRoom.CustomProperties["currentWave"];    // la ronda actual
                zombiesAlive = (int)PhotonNetwork.CurrentRoom.CustomProperties["zombiesAlive"]; // la cantidad de zombis en la ronda
                isBossWave = (bool)PhotonNetwork.CurrentRoom.CustomProperties["isBossWave"];   // si es la ronda del boss
                alivePlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["alivePlayers"];   // jugadores muertos en la partida

                OnWaveStarted?.Invoke(currentWave, zombiesAlive, isBossWave);
                OnZombiesAliveChanged?.Invoke(zombiesAlive);
                lvlUI.SetBossWarningActive(isBossWave);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (Input.GetKeyDown(KeyCode.P)) RoomLeaver.Instance.LeaveRoom();
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

        isBossWave = bossRound > 0 && currentWave % bossRound == 0; // setea si es la ronda del jefe

        int amount = isBossWave ? 1 : baseZombies + (currentWave - 1) * zombiesPerRound;    // si es la ronda del boss solo hay 1 
        zombiesAlive = amount;                                                              // sino es la cantidad de zombies base + los que se agregan x ronda
                                                    // y los zombies vivos es el resultado 
        Hashtable props = new Hashtable {{ "currentWave", currentWave }, { "zombiesAlive", zombiesAlive }, { "isBossWave", isBossWave }};// Actualiza las propiedades de la room,   
        PhotonNetwork.CurrentRoom.SetCustomProperties(props); //  asi cuando se cambia el ownership del master se puede continuar el juego  

        OnWaveStarted?.Invoke(currentWave, amount, isBossWave);
        OnZombiesAliveChanged?.Invoke(zombiesAlive);

        photonView.RPC(nameof(RPC_UpdateWaveInfo), RpcTarget.Others, currentWave, amount, isBossWave);
        photonView.RPC(nameof(RPC_UpdateZombiesAlive), RpcTarget.Others, zombiesAlive);
        photonView.RPC(nameof(RPC_ActivateBossWarning), RpcTarget.All, isBossWave);
    }

    public void OnZombieDied(bool wasBoss)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        zombiesAlive--;
        OnZombiesAliveChanged?.Invoke(zombiesAlive);

        // Actualiza la propiedad de la room de los zombis vivos
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "zombiesAlive", zombiesAlive } });

        photonView.RPC(nameof(RPC_UpdateZombiesAlive), RpcTarget.Others, zombiesAlive);
        LeaderboardService.AddScore(1, "kill_highscore");   // y suma el score a la leaderboard

        if (isBossWave) // si era la ronda del jefe 
        {
            if (wasBoss)    // y se murio victoria
                Victory();
        }
        else    // sino
        {
            if (zombiesAlive <= 0)  // no quedan zombis vivos siguiente ronda
                StartNextWave();
        }
    }

    private void HandlePlayerDied(int viewID)
    {
        if (deadPlayersIDs.Contains(viewID)) return;    // si el jugador registrado ya murio no hace nada

        deadPlayersIDs.Add(viewID); // sino lo agrega al y suma el contador
        deadPlayers++;

        alivePlayers = PhotonNetwork.CurrentRoom.PlayerCount - deadPlayers; // calcula la cantidad de jugadores vivos en la room
        OnAlivePlayersChanged?.Invoke(alivePlayers);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "alivePlayers", alivePlayers } });

        if (PhotonNetwork.IsMasterClient && alivePlayers <= 0)  // si es el master y ya no quedan jugadores vivos derrota
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
        if (PhotonNetwork.IsMasterClient)
            SceneLoader.LoadSceneByPhoton(ScenesEnum.Victory);
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
        lvlUI.SetBossWarningActive(isBossWave);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == null) return;    

        if (PhotonNetwork.LocalPlayer == newMasterClient)   // chequea si el jugador local se conviertio en el master
        {
            // obtiene los valores de la ronda de las propiedades de la room, asi se puede seguir jugando cuando se cambia de master
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentWave"))
            {
                currentWave = (int)PhotonNetwork.CurrentRoom.CustomProperties["currentWave"];
                zombiesAlive = (int)PhotonNetwork.CurrentRoom.CustomProperties["zombiesAlive"];
                isBossWave = (bool)PhotonNetwork.CurrentRoom.CustomProperties["isBossWave"];
                alivePlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["alivePlayers"];
            }

            OnWaveStarted?.Invoke(currentWave, zombiesAlive, isBossWave);   
            OnZombiesAliveChanged?.Invoke(zombiesAlive);
            lvlUI.SetBossWarningActive(isBossWave);
            OnAlivePlayersChanged?.Invoke(alivePlayers);

            // Transferir ownership de zombies
            ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
            if (spawner != null)
                spawner.TransferAllZombiesOwnership(newMasterClient);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!deadPlayersIDs.Contains(otherPlayer.ActorNumber))  // si el jugador que se fue ya estaba muerto no se le resta al contador de jugadores vivos
        {
            alivePlayers--;
            OnAlivePlayersChanged?.Invoke(alivePlayers);

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable{{"alivePlayers", alivePlayers}});
        }
    }
}
