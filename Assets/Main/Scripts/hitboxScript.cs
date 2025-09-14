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
        if (((1 << collision.gameObject.layer) & stats._attackLayer) != 0)
        {
            PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
            PhotonView myView = transform.root.GetComponent<PhotonView>();

            if (targetView && myView)
            {
                myView.RPC(nameof(EnemySyncScript.RPC_EnemyDamage), RpcTarget.All, targetView.ViewID, stats._damage); // llamo al rpc del gunsync para dañar a los zombies
            }

            Debug.Log(collision);
        }
    }
    
}