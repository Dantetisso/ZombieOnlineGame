using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MessageBroadcaster : MonoBehaviourPun // se encarga de conectar el messagedisplay con el photon view para poder llamar al rpc
{
    public static MessageBroadcaster Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void BroadcastMessageToAll(string msg)
    {
        photonView.RPC(nameof(RPC_ShowMessage), RpcTarget.All, msg);
    }

    [PunRPC]
    private void RPC_ShowMessage(string msg)
    {
        MessageDisplay.Instance?.AddMessage(msg);
    }
}

