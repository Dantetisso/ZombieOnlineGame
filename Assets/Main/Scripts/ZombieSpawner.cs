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

    private readonly Dictionary<int, bool> isBossByID = new();  // diccionario  que guarda el PhotonViewID y si el zombie es boss o no

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
        if (!PhotonNetwork.IsMasterClient) return;  // si no es el master no hace nada

        Transform[] spawnPoints = bossWave ? bossSpawnPoints : normalSpawnPoints;   // si es la ronda del boss va a spawnearlo en su spawnpoint
                                                                                // pero si es ronda normal, spawnea a los zombies en sus lugares correspondientes
        for (int i = 0; i < amount; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            GameObject go = PhotonNetwork.Instantiate(bossWave ? bossPrefab.name : zombiePrefab.name, spawnPoint.position, Quaternion.identity);

            ZombieController zombie = go.GetComponent<ZombieController>();  // al zombie instanciado le paso la referencia a este 
            zombie.SetSpawnerReference(this);                   // asi le avisa cuando muere, y poder actualizar al gameManager

            Transform[] waypoints = new Transform[spawnPoint.childCount];   // los waypoints son hijos de un gameobject, por eso recorre los hijos 
            for (int j = 0; j < spawnPoint.childCount; j++)
                waypoints[j] = spawnPoint.GetChild(j);

            zombie.SetPatrolWaypoints(waypoints);   // y se los asigna al zombie
            if (waypoints.Length > 0) zombie.EnablePatrol();    // si tiene waypoints hace que el zombie patrulle

            isBossByID[go.GetComponent<PhotonView>().ViewID] = bossWave;
        }
    }

    public void OnZombieDied(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool wasBoss = false;
        if (isBossByID.TryGetValue(viewID, out bool bossFlag))  // chequea en el diccionario si el zombie era el boss o no y lo elimina del diccionario
        {
            wasBoss = bossFlag;
            isBossByID.Remove(viewID);
        }

        gameManager?.OnZombieDied(wasBoss); // y le indica al gamemanager que murio un zombie
    }

    public void TransferAllZombiesOwnership(Player newMaster)   // al cambiar de Master
    {
        ZombieController[] zombies = FindObjectsOfType<ZombieController>(); // busca todos los zombies 
        foreach (var zombie in zombies)
        {
            PhotonView view = zombie.GetComponent<PhotonView>();    // obtiene su PhotonView
            if (view != null && !view.IsMine)   // chequea que tengan photon view y que no sea el jugador local
                view.TransferOwnership(newMaster);  // y transfiere el ownership
        }
    }
}
