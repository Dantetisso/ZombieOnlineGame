using System.Collections;
using System.Collections.Generic;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    public void StartGame() // si es el master carga el nivel
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SceneLoader.LoadScene(ScenesEnum.Level); 
        }
    }

    /*
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("true");
            photonView.RPC("LoadScene", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void LoadScene()
    {
        PhotonNetwork.LoadLevel("test");
        Dictionary<int, Player> players = ConnectionManager.Instance.GetPlayersInRoom();
        Debug.Log("Players count" + players.Count);
        int index = 0;

        foreach (KeyValuePair<int, Player> player in players) // x cada jugador pongo el texto
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint[index], Quaternion.identity);
            index++;
        }
    }

    // para cambiar de oleada se puede usar un rpc*/
}
