using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// Se encarga de instanciar zombis según la ronda y notificar al GameManager cuando mueren.
public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Puntos de Spawn")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Referencia a GameManager")]
    [SerializeField] private GameManager gameManager;

    // Guarda por ID si el zombi es boss para informar al manager
    private readonly Dictionary<int, bool> isBossByID = new();

    #region Unity Callbacks
    private void OnEnable()
    {
        GameManager.OnWaveStarted += SpawnWave;
    }

    private void OnDisable()
    {
        GameManager.OnWaveStarted -= SpawnWave;
    }
    #endregion

    #region Spawn Logic
    private void SpawnWave(int wave, int amount, bool bossWave)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados en ZombieSpawner.");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            string prefabName = bossWave ? "boss" : "zombie";
            Vector3 spawnPos = spawnPoints[i % spawnPoints.Length].position;

            GameObject go = PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);

            if (go.TryGetComponent<PhotonView>(out PhotonView pv))
            {
                isBossByID[pv.ViewID] = bossWave;
            }
            else
            {
                Debug.LogError($"El prefab {prefabName} no tiene PhotonView!");
            }
        }
    }
    #endregion

    #region Zombie Death
    /// Llamar desde el zombi cuando muere.
    public void OnZombieDied(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool wasBoss = false;
        if (isBossByID.TryGetValue(viewID, out bool bossFlag))
        {
            wasBoss = bossFlag;
            isBossByID.Remove(viewID);
        }

        if (gameManager != null)
        {
            gameManager.OnZombieDied(wasBoss);
        }
        else
        {
            Debug.LogError("GameManager no asignado en ZombieSpawner!");
        }
    }
    #endregion
}
