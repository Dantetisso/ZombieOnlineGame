using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomListUI : MonoBehaviour
{
    [System.Serializable]
    public class RoomSlot
    {
        public TMP_Text roomNameText;
        public TMP_Text playerCountText;
        public Button joinButton;
    }

    [Header("Slots de salas")]
    [SerializeField] public RoomSlot[] slots; 

    private void OnEnable() // se llama cuando se activa el GAMEOBJECT que tiene este script y se suscrube al evento OnNewRoomCreated basicamente para actualizar la lista de rooms
    {
        if (ConnectionManager.Instance != null)
            ConnectionManager.Instance.photonManager.OnNewRoomCreated += UpdateRoomList;
    }

    private void OnDisable()
    {
        if (ConnectionManager.Instance != null) // lo contrario al OnEnable
            ConnectionManager.Instance.photonManager.OnNewRoomCreated -= UpdateRoomList;
    }

    public void UpdateRoomList(List<RoomInfo> rooms)
    {
        // primero "inicializa" los slots
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].roomNameText.text = "Empty";
            slots[i].playerCountText.text = "Players: 0/4";
            slots[i].joinButton.gameObject.SetActive(false);
            slots[i].joinButton.onClick.RemoveAllListeners();
        }

        // y los llena con las rooms disponibles
        for (int i = 0; i < rooms.Count && i < slots.Length; i++)
        {
            RoomInfo room = rooms[i];
            RoomSlot slot = slots[i];

            slot.roomNameText.text = room.Name;
            slot.playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";

            if (room.PlayerCount < room.MaxPlayers) // si la cantidad de jugadores es menor al maximo
            {
                slot.joinButton.gameObject.SetActive(true); // prende el boton de join y lo hace interactuable
                slot.joinButton.interactable = true;
                slot.joinButton.onClick.RemoveAllListeners();
                slot.joinButton.onClick.AddListener(() =>
                {
                    ConnectionManager.Instance.JoinSelectedRoom(room.Name);     // y te une a esa room
                    MainMenuStarter.hasRequestedJoinRoom = true;
                });
            }
        }
    }
}
