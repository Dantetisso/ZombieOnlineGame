using System.Collections.Generic;
using Photon.Pun;
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

        if (!bossWave)
        {
            for (int i = 0; i < amount; i++)
            {
                Transform spawnPoint = normalSpawnPoints[i % normalSpawnPoints.Length];
                GameObject go = PhotonNetwork.Instantiate(zombiePrefab.name, spawnPoint.position, Quaternion.identity);
                if (go.TryGetComponent(out ZombieController zombieCtrl))
                {
                    // Asignar waypoints hijos
                    Transform[] waypoints = new Transform[spawnPoint.childCount];
                    for (int j = 0; j < spawnPoint.childCount; j++)
                        waypoints[j] = spawnPoint.GetChild(j);

                    zombieCtrl.SetPatrolWaypoints(waypoints);
                    zombieCtrl.EnablePatrol();

                    isBossByID[go.GetComponent<PhotonView>().ViewID] = false;
                }
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                Transform spawnPoint = bossSpawnPoints[i % bossSpawnPoints.Length];
                GameObject go = PhotonNetwork.Instantiate(bossPrefab.name, spawnPoint.position, Quaternion.identity);
                if (go.TryGetComponent(out ZombieController bossCtrl))
                {
                    Transform[] waypoints = new Transform[spawnPoint.childCount];
                    for (int j = 0; j < spawnPoint.childCount; j++)
                        waypoints[j] = spawnPoint.GetChild(j);

                    bossCtrl.SetPatrolWaypoints(waypoints);
                    if (waypoints.Length > 0) bossCtrl.EnablePatrol(); // patrulla solo si tiene hijos
                    isBossByID[go.GetComponent<PhotonView>().ViewID] = true;
                }
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
