using UnityEngine;

public static class LobbyMesenger   // se llama a esta clase cuando se quiere invocar algun mensaje
{
    public static void PlayerEnterRoomMessage(string player)
    {
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"{player} entered the room.", Color.green);
    } 
    
    public static void PlayerLeftRoomMessage(string player)
    {
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"{player} left the room.", Color.red);
    } 

    public static void PlayerLeftMessage()
    {
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"You left the room.", Color.red);
    }

    public static void PlayerDeadMessage(string player)
    { 
        MessageBroadcaster.Instance?.BroadcastMessageToAll($"{player} died.", Color.red);
    } 
}
