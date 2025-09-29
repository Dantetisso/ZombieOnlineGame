using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviourPunCallbacks // conecta entre el juego y photon
{
    public static ConnectionManager Instance { get; private set; }
    public PhotonConnectionManager photonManager;
    private Action OnConnectedToServer;
    public Action OnJoinRoom;
    public Action OnPlayerEnterRoom;
    public Action OnPlayerLeaveRoom;

    private List<RoomInfo> rooms = new List<RoomInfo>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        PhotonNetwork.AutomaticallySyncScene = true; // para que le cargue a todos los jugadores a la vez
    }

    void Start()
    {
        photonManager.Init(HandleJoinedRoom, HandleRoomCreated, HandleNewPlayerInRoom, HandlePlayerLeftRoom);
    }

    #region Metodos
    public void SetNickName(string NickName)
    {
        photonManager.SetNickName(NickName);
    }

    public void ConnectToServer(Action connectionCallback = null)
    {
        photonManager.ConnectToServer(HandleConnectionToServer);
        OnConnectedToServer += connectionCallback;
    }

    private void HandleConnectionToServer()
    {
        OnConnectedToServer?.Invoke();
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    private void HandleJoinedRoom()
    {
        OnJoinRoom?.Invoke();
    }

    public void JoinSelectedRoom(string roomName)
    {
        photonManager.JoinRoom(roomName);
    }

    public void JoinRoom(string roomName)
    {
        photonManager.JoinRoom(roomName);
    }

    public void HandleNewPlayerInRoom(Player player)
    {
        OnPlayerEnterRoom?.Invoke();
        LobbyMesenger.PlayerEnterMessage(player.NickName);
    }
                                                            // manejan la entrada y salida de jugadores
    public void HandlePlayerLeftRoom(Player player)
    {
        OnPlayerLeaveRoom?.Invoke();
        LobbyMesenger.PlayerLeftMessage(player.NickName);
    }

    public void CreateRoom(string roomName) //el room options esta en el script photonconectionmanager
    {
        photonManager.CreateRoom(roomName);
    }

    public void HandleRoomCreated(List<RoomInfo> rooms)
    {
        this.rooms = rooms;
    }

    public List<RoomInfo> GetAllRooms()
    {
        return rooms;
    }

    public string GetCurrentRoomName()
    {
        string name = photonManager.GetCurrentRoom().Name;
        return name;
    }

    public Dictionary<int, Player> GetPlayersInRoom()
    {
        return photonManager.GetPlayersInRoom();
    }

    private bool hasRequestedJoinRoom = false;

    public void SetHasRequestedJoinRoom(bool value)
    {
        hasRequestedJoinRoom = value;
    }

    public bool GetHasRequestedJoinRoom()
    {
        return hasRequestedJoinRoom;
    }
    

    #endregion

}
