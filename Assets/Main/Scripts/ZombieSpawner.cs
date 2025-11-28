using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] normalSpawnPoints;
    [SerializeField] private Transform[] bossSpawnPoints;

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject bossPrefab;

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

        Transform[] spawnPoints = bossWave ? bossSpawnPoints : normalSpawnPoints;

        for (int i = 0; i < amount; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            GameObject go = PhotonNetwork.Instantiate(bossWave ? bossPrefab.name : zombiePrefab.name, spawnPoint.position, Quaternion.identity);

            ZombieController zombie = go.GetComponent<ZombieController>();
            zombie.SetSpawnerReference(this);

            Transform[] waypoints = new Transform[spawnPoint.childCount];
            for (int j = 0; j < spawnPoint.childCount; j++)
                waypoints[j] = spawnPoint.GetChild(j);

            zombie.SetPatrolWaypoints(waypoints);
            if (waypoints.Length > 0) zombie.EnablePatrol();

            isBossByID[go.GetComponent<PhotonView>().ViewID] = bossWave;
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

    public void TransferAllZombiesOwnership(Player newMaster)
    {
        ZombieController[] zombies = FindObjectsOfType<ZombieController>();
        foreach (var zombie in zombies)
        {
            PhotonView view = zombie.GetComponent<PhotonView>();
            if (view != null && !view.IsMine)
                view.TransferOwnership(newMaster);
        }
    }
}
