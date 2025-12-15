using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

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
    public GameObject warningtext;
    private float warningTime = 2f;

    public bool canplayAlone = false;

    private void OnEnable()
    {
        if (ConnectionManager.Instance != null)
            ConnectionManager.Instance.OnRoomListChanged += RefreshUI;
    
        RefreshUI();
    }

    private void OnDisable()
    {
        if (ConnectionManager.Instance != null)
            ConnectionManager.Instance.OnRoomListChanged -= RefreshUI;
    }

    private void SafeUpdateRoomList(List<RoomInfo> rooms) // asegurar de limpiar suscripciones 
    {
        if (this == null || gameObject == null) return;
        RefreshUI();
    }

    public void UpdateRoomList(List<RoomInfo> rooms)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].roomNameText.text = "Empty";
            slots[i].playerCountText.text = "Players: 0 / 4";
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
                    
                    if (room.PlayerCount >= room.MaxPlayers && slot.joinButton.IsInvoking()) StartCoroutine(WarningText());
                });
            }
        }
    }

    public List<RoomInfo> GetRoomsByHostNickname(List<RoomInfo> rooms, string nickname)
    {
        List<RoomInfo> result = new List<RoomInfo>();
        
        foreach (RoomInfo room in rooms)
        {
            if (room.CustomProperties.TryGetValue("HostNick", out object host))
            {
                if (host.ToString().ToLower().Contains(nickname.ToLower()))
                {
                    result.Add(room);
                }
            }
        }

        return result;
    }

    public void RefreshUI()
    {
        if (ConnectionManager.Instance == null) return;

        // 🔥 SI ESTOY EN UNA ROOM, NO ACTUALIZO LISTA
        if (PhotonNetwork.InRoom)
            return;

        List<RoomInfo> allRooms = ConnectionManager.Instance.GetAllRoomsCached();

        // 🔥 FILTRAR ROOMS VALIDAS
        List<RoomInfo> validRooms = new List<RoomInfo>();
        foreach (RoomInfo room in allRooms)
        {
            if (!room.RemovedFromList)
                validRooms.Add(room);
        }

        UpdateRoomList(validRooms);
    }

    IEnumerator WarningText()
    {
        warningtext.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        warningtext.SetActive(false);
    }

}
