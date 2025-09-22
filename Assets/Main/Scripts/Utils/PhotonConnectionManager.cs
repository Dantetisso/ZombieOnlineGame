using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonConnectionManager : MonoBehaviourPunCallbacks
{
    public Action OnConnectedToServer;
    public Action OnJoinedRoomEvent;
    public Action<List<RoomInfo>> OnNewRoomCreated;
    public Action<Player> OnPlayerEnteredRoomEvent;
    public Action<Player> OnPlayerLeftRoomEvent;

    public void Init(Action onJoinRoom, Action<List<RoomInfo>> onRoomCreated, Action<Player> onPlayerEnterRomCallback, Action<Player> onPlayerleftCallback)
    {
        OnJoinedRoomEvent += onJoinRoom;
        OnNewRoomCreated += onRoomCreated;

        OnPlayerEnteredRoomEvent += onPlayerEnterRomCallback;
        OnPlayerLeftRoomEvent += onPlayerleftCallback;
    }

    public void SetNickName(string NickName)
    {
        PhotonNetwork.NickName = NickName;
    }

    public void ConnectToServer(Action onConnect = null)
    {
        PhotonNetwork.ConnectUsingSettings();
        OnConnectedToServer += onConnect;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToServer");
        OnConnectedToServer?.Invoke();
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        OnJoinedRoomEvent?.Invoke();
        Debug.Log("OnJoinedRoom");
    }

    public Room GetCurrentRoom() { return PhotonNetwork.CurrentRoom; }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnNewRoomCreated?.Invoke(roomList);
    }

    public void LeaveRoom() { PhotonNetwork.LeaveRoom(); }

    public Dictionary<int, Player> GetPlayersInRoom()
    {
        return PhotonNetwork.CurrentRoom.Players;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerEnteredRoomEvent?.Invoke(newPlayer);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        OnPlayerLeftRoomEvent?.Invoke(otherPlayer);
    }

}

