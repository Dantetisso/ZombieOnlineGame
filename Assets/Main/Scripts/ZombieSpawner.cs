using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Spawner Settings")]
    [SerializeField] private GameConfig config;

    [Header("Spawn Points")]
    [Tooltip("Spawn points para zombies normales")]
    [SerializeField] private Transform[] normalSpawnPoints;

    [Tooltip("Spawn point único para el boss")]
    [SerializeField] private Transform bossSpawnPoint;

    [Header("Prefabs")]
    [SerializeField] private GameObject normalZombiePrefab;
    [SerializeField] private GameObject bossZombiePrefab;

    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    private readonly Dictionary<int, bool> isBossByID = new();

    private void OnEnable() => GameManager.OnWaveStarted += SpawnWave;
    private void OnDisable() => GameManager.OnWaveStarted -= SpawnWave;

    private void SpawnWave(int wave, int amount, bool bossWave)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < amount; i++)
        {
            bool spawnBoss = bossWave && i == 0;
            GameObject prefab = spawnBoss ? bossZombiePrefab : normalZombiePrefab;

            // Spawn point
            Transform spawnPoint = spawnBoss ? bossSpawnPoint : normalSpawnPoints[i % normalSpawnPoints.Length];

            GameObject go = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, Quaternion.identity);

            if (go.TryGetComponent(out PhotonView pv))
                isBossByID[pv.ViewID] = spawnBoss;

            if (go.TryGetComponent(out ZombieController controller))
            {
                // waypoints de hijos del spawn point
                Transform[] waypoints = new Transform[spawnPoint.childCount];
                for (int w = 0; w < spawnPoint.childCount; w++)
                    waypoints[w] = spawnPoint.GetChild(w);

                controller.SetWaypoints(waypoints);

                if (spawnBoss)
                    controller.DisablePatrol();
            }
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
