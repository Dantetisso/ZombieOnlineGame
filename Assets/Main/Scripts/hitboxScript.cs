using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hitboxScript : MonoBehaviourPunCallbacks
{
    [SerializeField] EnemyStats stats;
    [SerializeField] LayerMask targetlayer;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // if (!PhotonNetwork.IsMasterClient) return;

        if (((1 << collision.gameObject.layer) & stats._attackLayer) != 0)
        {
            PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
            PhotonView myView = transform.root.GetComponent<PhotonView>(); // busco el photon view del padre (en este caso el jugador xq las armas son hijos)

            if (targetView && myView)
            {
                // ⚔️ Envía el daño al dueño del jugador
                targetView.RPC(nameof(PlayerGunSync.RPC_MakeDamage), RpcTarget.MasterClient, myView.ViewID, stats._damage); // llamo al rpc del gunsync para dañar a los zombies
            }
           
        }
    }
  
    
}