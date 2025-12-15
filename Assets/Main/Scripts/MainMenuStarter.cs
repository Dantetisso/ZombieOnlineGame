using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
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

    [Header("Warning Nickname Text")]
    [SerializeField] private GameObject warningNicknameText;
    [SerializeField] private float warningTime;

    [Header("Create Room UI")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button createRoomConfirmButton;

    [Header("Join Panel Slots Fijos")]
    [SerializeField] private RoomListUI.RoomSlot[] roomSlots; // slots fijos asignados en inspector
    private RoomListUI roomListUI;

    [Header("Search Room UI")]
    [SerializeField] private TMP_InputField searchPlayerInput;
    [SerializeField] private Button searchRoomButton;

    [SerializeField] private GameObject searchResultPanel;
    [SerializeField] private RoomListUI searchRoomListUI;
    [SerializeField] private GameObject searchErrorText;
    [SerializeField] private Button searchBackButton;

    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;
        hasRequestedJoinRoom = false;

        PhotonNetwork.NickName = "";
        playerNameInput.text = "";

        connectButton.onClick.AddListener(OnConnectButtonClicked);
        createRoomConfirmButton.onClick.AddListener(OnCreateRoomConfirmed);
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);

        searchRoomButton.onClick.AddListener(OnSearchRoomClicked);
        searchBackButton.onClick.AddListener(OnSearchBackClicked);

        if (searchResultPanel != null)
        {
            searchResultPanel.SetActive(false);
        }

        if (searchErrorText != null)
        {
            searchErrorText.SetActive(false);
        }

        joinPanel.SetActive(false);
        createRoomPanel.SetActive(false);

        roomListUI = joinPanel.GetComponent<RoomListUI>();

        if (roomListUI != null && roomSlots.Length > 0)
        {
            roomListUI.slots = roomSlots;
        }

        if (searchRoomListUI != null && roomSlots.Length > 0)
        {
            searchRoomListUI.slots = roomSlots;
        }
    }

    void OnConnectButtonClicked()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            StartCoroutine(warningText());
            return;
        }

        ConnectionManager.Instance.SetNickName(playerName);
        PlayerNameHelper.SetPlayerName(playerName);

        if (!PhotonNetwork.IsConnected)
        {
            var self = this;

            ConnectionManager.Instance.ConnectToServer(() =>
            {
                if (self == null || self.gameObject == null) return;

                Debug.Log("Connected to server, showing rooms");
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
            roomName = PhotonNetwork.NickName + "'s Room";
        }

        ConnectionManager.Instance.CreateRoom(roomName);
        hasRequestedJoinRoom = true;
        createRoomPanel.SetActive(false);
    }

    private void OnSearchRoomClicked()
    {
        if (searchErrorText != null)
            searchErrorText.SetActive(false);

        if (searchResultPanel != null)
            searchResultPanel.SetActive(false);

        string searchText = searchPlayerInput.text.Trim();
        if (string.IsNullOrEmpty(searchText))
            return;

        List<RoomInfo> allRooms = ConnectionManager.Instance.GetAllRoomsCached();
        List<RoomInfo> foundRooms = new List<RoomInfo>();

        foreach (RoomInfo room in allRooms)
        {
            if (room.CustomProperties != null &&
                room.CustomProperties.TryGetValue("HostNick", out object host))
            {
                if (host.ToString().ToLower().Contains(searchText.ToLower()))
                {
                    foundRooms.Add(room);
                }
            }
        }

        if (foundRooms.Count == 0)
        {
            if (searchErrorText != null)
                searchErrorText.SetActive(true);
            return;
        }

        joinPanel.SetActive(false);
        searchResultPanel.SetActive(true);

        searchRoomListUI.UpdateRoomList(foundRooms);
    }

    private void OnSearchBackClicked()
    {
        searchResultPanel.SetActive(false);
        joinPanel.SetActive(true);
        
        roomListUI.RefreshUI();

        if (searchErrorText != null)
            searchErrorText.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        if (hasRequestedJoinRoom)
        {
            hasRequestedJoinRoom = false;
            SceneLoader.LoadScene(ScenesEnum.Lobby);
        }
    }

    IEnumerator warningText()
    {
        warningNicknameText.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        warningNicknameText.SetActive(false);
    }

    void OnDestroy()
    {
        if (connectButton != null)
            connectButton.onClick.RemoveAllListeners();

        if (createRoomConfirmButton != null)
            createRoomConfirmButton.onClick.RemoveAllListeners();

        if (createRoomButton != null)
            createRoomButton.onClick.RemoveAllListeners();

        if (searchRoomButton != null)
            searchRoomButton.onClick.RemoveAllListeners();

        if (searchBackButton != null)
            searchBackButton.onClick.RemoveAllListeners();
    }
}
