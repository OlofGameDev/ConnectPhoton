using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//using Hashtable = ExitGames.Client.Photon.Hashtable;

// MonoBehaviorPunCallbacks gives us the MonoBehavior stuff and allows us to override the PunCallbacks
public class ConnectPhoton : MonoBehaviourPunCallbacks
{
    public static ConnectPhoton master;

    private const string GameVersion = "0.1";

    // Start is called before the first frame update
    #region Monobehavior Methods
    private void Awake()
    {
        if (master != null) Destroy(this);
        master = this;
        // Keep this gameobject between scene changes
        DontDestroyOnLoad(gameObject);
        // If you are in a room and the owner loads a new scene it will load the same scene for everyone in the room
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        // Connect to the server
        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    #endregion Monobehavior Methods

    #region Custom Methods
    public void CreateRoom(string roomName, int maxPlayers)
    {
        // Create a new RoomOptions
        RoomOptions thisRoomOptions = new RoomOptions() { MaxPlayers = (byte)maxPlayers, PublishUserId = true };
        // Create a hash table for custom options
        ExitGames.Client.Photon.Hashtable customOptions = new ExitGames.Client.Photon.Hashtable();
        // Create a room
        PhotonNetwork.CreateRoom(roomName, thisRoomOptions );
    }
    public void JoinRoom(string roomName)
    {
        //PhotonServerListings.master.SetServerMessage("Attempting to join room.", PhotonServerListings.master.GetLocalMessageColor);
        if(PhotonNetwork.InLobby) PhotonNetwork.JoinRoom(roomName);
    }
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
    public void ChangeNickName(string newName)
    {
        PhotonNetwork.NickName = newName;
    }
    #endregion Custom Methods

    #region Photon Override methods
    public override void OnConnectedToMaster()
    {
        PhotonServerListings.master.ConnectedToMaster();
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        PhotonServerListings.master.MasterClientSwitched(newMasterClient);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        PhotonServerListings.master.CreatedRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        PhotonServerListings.master.JoinRoomFailed(returnCode, message);
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonServerListings.master.JoinedRoom(PhotonNetwork.CurrentRoom.Name);
        
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.JoinLobby();
        PhotonServerListings.master.LeftRoom();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        PhotonServerListings.master.PlayerPropertiesUpdate(targetPlayer, changedProps);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        PhotonServerListings.master.PlayerLeftRoom(otherPlayer);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        // When a player enters the room, check if the current players equals the max players set for the room
        if (PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            // Close the room
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        PhotonServerListings.master.PlayerEnteredRoom(newPlayer);

    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected");
        PhotonServerListings.master.Disconnected(cause);

    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        PhotonServerListings.master.CreateRoomFailed(returnCode, message);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        PhotonServerListings.master.RoomListUpdated(roomList);
    }

    #endregion Photon Override methods

}
