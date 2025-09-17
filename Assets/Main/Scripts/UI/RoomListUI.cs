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

    private void OnEnable()
    {
        if (ConnectionManager.Instance != null)
            ConnectionManager.Instance.photonManager.OnNewRoomCreated += SafeUpdateRoomList;
    }

    private void OnDisable()
    {
        if (ConnectionManager.Instance != null)
            ConnectionManager.Instance.photonManager.OnNewRoomCreated -= SafeUpdateRoomList;
    }

    private void SafeUpdateRoomList(List<RoomInfo> rooms) // asegurar de limpiar suscripciones 
    {
        if (this == null || gameObject == null) return;
        UpdateRoomList(rooms);
    }

    public void UpdateRoomList(List<RoomInfo> rooms)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].roomNameText.text = "Empty";
            slots[i].playerCountText.text = "Players: 0/4";
            slots[i].joinButton.gameObject.SetActive(false);
            slots[i].joinButton.onClick.RemoveAllListeners();
        }

        for (int i = 0; i < rooms.Count && i < slots.Length; i++)
        {
            RoomInfo room = rooms[i];
            RoomSlot slot = slots[i];

            slot.roomNameText.text = room.Name;
            slot.playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";

            if (room.PlayerCount < room.MaxPlayers)
            {
                slot.joinButton.gameObject.SetActive(true);
                slot.joinButton.interactable = true;
                slot.joinButton.onClick.RemoveAllListeners();
                slot.joinButton.onClick.AddListener(() =>
                {
                    if (ConnectionManager.Instance != null)
                    {
                        ConnectionManager.Instance.JoinSelectedRoom(room.Name);
                        MainMenuStarter.hasRequestedJoinRoom = true;
                    }
                });
            }
        }
    }
}
