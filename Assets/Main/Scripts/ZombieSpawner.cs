using Photon.Pun;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using TMPro;

public class ZombieSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] int maxWaves; //5

    [Header("Configuración de zombis por ronda")]
    [SerializeField] int baseZombies; //5
    [SerializeField] int zombiesPerRound; //2
    [SerializeField] int bossRound; //5

    [SerializeField] TMP_Text waveText;
    [SerializeField] TMP_Text aliveText;

    int currentWave;
    int zombiesAlive;

    readonly Dictionary<int, bool> isBossByID = new();

    void Start()
    {
        if (PhotonNetwork.IsMasterClient) StartNextWave();
    }

    void StartNextWave()
    {
        currentWave++;
        if (currentWave > maxWaves) return;

        bool bossWave = currentWave % bossRound == 0;
        int amount    = bossWave ? 1 : baseZombies + (currentWave - 1) * zombiesPerRound;

        zombiesAlive = amount;

        for (int i = 0; i < amount; i++)
        {
            string prefab = bossWave ? "boss" : "zombie";
            Vector3 pos   = spawnPoints[i % spawnPoints.Length].position;

            GameObject go = PhotonNetwork.Instantiate(prefab, pos, quaternion.identity);
            int id        = go.GetComponent<PhotonView>().ViewID;
            isBossByID[id] = bossWave;          // true si es boss
        }

        photonView.RPC(nameof(RPC_SetWaveUI), RpcTarget.AllBuffered, currentWave, zombiesAlive);
    }

    public void OnZombieDied(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        zombiesAlive--;
        bool wasBoss = isBossByID.TryGetValue(viewID, out bool bossFlag) && bossFlag;
        isBossByID.Remove(viewID);

        if (wasBoss)
        {
            photonView.RPC(nameof(RPC_Victory), RpcTarget.All);
            return;
        }

        photonView.RPC(nameof(RPC_SetAliveUI), RpcTarget.All, zombiesAlive);

        if (zombiesAlive <= 0) StartNextWave();
    }

    [PunRPC]
    void RPC_SetWaveUI(int wave, int alive)
    {
        waveText.text  = $"Round {wave}";
        aliveText.text = alive.ToString();
    }

    [PunRPC]
    void RPC_SetAliveUI(int alive)
    {
        aliveText.text = alive.ToString();
    }

    [PunRPC]
    void RPC_Victory()
    {
        // SOLO el Master llama carga el nivel (los demás se sincronizan gracias a AutomaticallySyncScene)
        if (PhotonNetwork.IsMasterClient)
        {
            SceneLoader.LoadSceneByPhoton(ScenesEnum.Victory);
        }
    }
}
