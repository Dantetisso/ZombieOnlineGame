using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Collections.Generic;

public class MainMenuStarter : MonoBehaviourPunCallbacks
{
    public static bool hasRequestedJoinRoom = false;

    [Header("Main Menu")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button connectButton;

    [Header("Panels")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject joinPanel;       // panel de rooms
    [SerializeField] private GameObject createRoomPanel; // panel para crear room
    [SerializeField] private Button createRoomButton;

    [Header("Create Room UI")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button createRoomConfirmButton;

    [Header("Join Panel Slots Fijos")]
    [SerializeField] private RoomListUI.RoomSlot[] roomSlots; // slots fijos asignados en inspector
    private RoomListUI roomListUI;

    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;
        hasRequestedJoinRoom = false;

        PhotonNetwork.NickName = "";
        playerNameInput.text = "";

        connectButton.onClick.AddListener(OnConnectButtonClicked);
        createRoomConfirmButton.onClick.AddListener(OnCreateRoomConfirmed);
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);

        joinPanel.SetActive(false);
        createRoomPanel.SetActive(false);

        // Tomo el RoomListUI desde el panel
        roomListUI = joinPanel.GetComponent<RoomListUI>();

        // Asignar slots desde inspector
        if (roomListUI != null && roomSlots.Length > 0)
            roomListUI.slots = roomSlots;
    }

    void OnConnectButtonClicked()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Sin nickname no entras.");
            return;
        }

        ConnectionManager.Instance.SetNickName(playerName);

        if (!PhotonNetwork.IsConnected)
        {
            // Guardamos una referencia local de este objeto
            var self = this;

            ConnectionManager.Instance.ConnectToServer(() =>
            {
                // Chequeamos si el objeto sigue existiendo
                if (self == null || self.gameObject == null) return;

                Debug.Log("Conectado al servidor. Mostrando rooms...");
                PhotonNetwork.JoinLobby();
                self.connectPanel.SetActive(false);
                self.joinPanel.SetActive(true);
            });
        }
        else
        {
            PhotonNetwork.JoinLobby();
            connectPanel.SetActive(false);
            joinPanel.SetActive(true);
        }
    }

    public void OnCreateRoomButtonClicked()
    {
        joinPanel.SetActive(false);
        createRoomPanel.SetActive(true);
    }

    void OnCreateRoomConfirmed()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        string roomName = roomNameInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = PhotonNetwork.NickName + "'s Room"; // si no pones un nombre te pone este por defecto
        }

        // Creo la sala con max 4 jugadores
        ConnectionManager.Instance.CreateRoom(roomName);
        hasRequestedJoinRoom = true;
        createRoomPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        if (hasRequestedJoinRoom)
        {
            hasRequestedJoinRoom = false;
            SceneLoader.LoadScene(ScenesEnum.Lobby); // el jugador entra a la escena del lobby
        }
    }

    void OnDestroy()
    {
        if (connectButton != null)
            connectButton.onClick.RemoveAllListeners();

        if (createRoomConfirmButton != null)
            createRoomConfirmButton.onClick.RemoveAllListeners();

        if (createRoomButton != null)
            createRoomButton.onClick.RemoveAllListeners();
    }
}
