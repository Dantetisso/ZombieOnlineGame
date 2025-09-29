using UnityEngine;
using Photon.Pun;

public class MessageBroadcaster : MonoBehaviourPun // se encarga de conectar el messagedisplay con el photon view para poder llamar al rpc
{
    public static MessageBroadcaster Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void BroadcastMessageToAll(string msg, Color color)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        photonView.RPC(nameof(RPC_ShowMessage), RpcTarget.All, msg, colorHex);
    }

    [PunRPC]
    private void RPC_ShowMessage(string msg, string colorHex)
    {
        if (!ColorUtility.TryParseHtmlString($"#{colorHex}", out var color))
            color = Color.white;

        MessageDisplay.Instance?.AddMessageWithColor(msg, color);
    }
}

