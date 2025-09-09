using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private Transform[] spawnPoints;   

    void Start() // va instanciando a los jugadores en los puntos x orden 
    {
        if (!PhotonNetwork.InRoom) return;

        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;       
        Vector3 spawnPos = spawnPoints[index % spawnPoints.Length].position;

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
    }
}
