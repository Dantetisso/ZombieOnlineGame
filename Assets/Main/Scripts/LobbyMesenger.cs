using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public static class LobbyMesenger
{
    public static void PlayerLeftMessage(string player)
    {
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"{player} left the room.");
    } 

    public static void PlayerEnterMessage(string player)
    {
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"{player} entered the room.");
    } 


    public static void PlayerDeadMessage(string player)
    { 
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"{player} died.");
    } 
}
