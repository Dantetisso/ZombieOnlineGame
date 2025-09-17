using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameManager gameManager;

    private readonly Dictionary<int, bool> isBossByID = new();

    private void OnEnable()
    {
        GameManager.OnWaveStarted += SpawnWave;
    }

    private void OnDisable()
    {
        GameManager.OnWaveStarted -= SpawnWave;
    }

    private void SpawnWave(int wave, int amount, bool bossWave)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < amount; i++)
        {
            // Solo un boss por ronda
            string prefabName = bossWave ? "boss" : "zombie";
            Vector3 spawnPos = spawnPoints[i % spawnPoints.Length].position;

            GameObject go = PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);
            if (go.TryGetComponent<PhotonView>(out PhotonView pv))
                isBossByID[pv.ViewID] = bossWave;
        }
    }

    public void OnZombieDied(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool wasBoss = false;
        if (isBossByID.TryGetValue(viewID, out bool bossFlag))
        {
            wasBoss = bossFlag;
            isBossByID.Remove(viewID);
        }

        gameManager?.OnZombieDied(wasBoss);
    }
}
