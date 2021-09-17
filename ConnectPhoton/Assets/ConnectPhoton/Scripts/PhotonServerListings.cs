using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using Photon.Realtime;
using Photon.Pun;
//using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum StringType
{
    UserName,
    RoomName,
    MaxPlayers,
    GameType
}
public class PhotonServerListings : MonoBehaviour
{
    public static PhotonServerListings master;

    [Header("'Server' messages")]
    [SerializeField] Transform serverMessageContent;
    [SerializeField] GameObject serverMessagePrefab;
    [SerializeField] Color serverSideMessageColor;
    [SerializeField] Color localMessageColor;
    public Color GetLocalMessageColor { get { return localMessageColor; } }

    [Header("Not In Room")]
    [SerializeField] TextMeshProUGUI connectingText;
    [SerializeField] Transform notInRoomPanel;
    [SerializeField] Transform roomsListingContent;
    [SerializeField] TMP_InputField createRoomInputField;
    [SerializeField] TMP_InputField userName;
    [SerializeField] TMP_Dropdown maxPlayers;
    [SerializeField] TMP_Dropdown gameType;
    [SerializeField] Button createRoomButton;
    [SerializeField] GameObject roomListingPrefab;
    [SerializeField] List<RoomListing> roomListings;

    [Header("In Room")]
    [SerializeField] Transform inRoomPanel;
    [SerializeField] Transform playerListingContent;
    [SerializeField] Button startButton;
    [SerializeField] TMP_Dropdown team;
    [SerializeField] GameObject playerListingPrefab;
    [SerializeField] List<PlayerListing> playerListings;
    [SerializeField] TextMeshProUGUI roomName;
    [SerializeField] TextMeshProUGUI playersInRoom;
    [SerializeField] Toggle readyToggle;

    bool initMaxPlayersComplete = false;
    bool initGameTypeComplete = false;

    float roomListUpdateCooldownTimer;

    #region Monobehaviors
    private void Awake()
    {
        if (master != null) Destroy(this);
        master = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        createRoomButton.interactable = false;
        SetUpInputFields(StringType.UserName);
        SetUpInputFields(StringType.RoomName);
        notInRoomPanel.gameObject.SetActive(true);
        inRoomPanel.gameObject.SetActive(false);
        SetGameTypeOptions();
    }
    // Update is called once per frame
    void Update()
    {
        if (roomListUpdateCooldownTimer > 0) roomListUpdateCooldownTimer -= Time.deltaTime;
    }

    #endregion Monobehaviors

    #region UI functions
    /// <summary>
    ///  Updates the max-player-values to match the selected gametype
    /// </summary>
    public void UpdateMaxPlayers()
    {
        initMaxPlayersComplete = false;
        GameType[] games = PhotonGames.master.ReturnGames;
        int roomMinPlayers = games[gameType.value].minPlayers;
        int roomMaxPlayers = games[gameType.value].maxPlayers;

        maxPlayers.ClearOptions();
        for(int i = roomMinPlayers; i < roomMaxPlayers + 1; i += 2)
        {
            TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
            newData.text = i.ToString();
            maxPlayers.options.Add(newData);
        }
        maxPlayers.RefreshShownValue();
        ChangedMaxPlayers();
        initMaxPlayersComplete = true;

    }
    private void SetUpInputFields(StringType type)
    {
        if (type == StringType.UserName)
        {
            if (!PlayerPrefs.HasKey("UserName"))
            {
                userName.text = "New User";
                return;
            }
            string storedName = PlayerPrefs.GetString("UserName");
            ConnectPhoton.master.ChangeNickName(storedName);
            userName.text = storedName;
        }
        else if (type == StringType.RoomName)
        {
            if (!PlayerPrefs.HasKey("RoomName"))
            {
                createRoomInputField.text = "New Room";
                return;
            }
            string storedName = PlayerPrefs.GetString("RoomName");
            createRoomInputField.text = storedName;
        }
    }
    
    public void SavePlayerName()
    {
        StringCheck(StringType.UserName);
        ConnectPhoton.master.ChangeNickName(PlayerPrefs.GetString("UserName"));
    }
    public void SaveRoomName()
    {
        StringCheck(StringType.RoomName);
    }
    public void ChangedMaxPlayers()
    {
        if (!initMaxPlayersComplete) return;
        GameType[] games = PhotonGames.master.ReturnGames;
        int maxPlayersInRoom = games[gameType.value].minPlayers + (maxPlayers.value * 2);
        SetServerMessage($"Changed max players to {maxPlayersInRoom}.", localMessageColor);
    }
    public void ChangedGameType()
    {
        if (!initGameTypeComplete) return;

        GameType[] games = PhotonGames.master.ReturnGames;

        SetServerMessage($"Changed game type to {games[gameType.value].name}.", localMessageColor);
        UpdateMaxPlayers();
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void StartGame()
    {
        // Only the master client can start the game
        if (!PhotonNetwork.IsMasterClient) return;
        // All players in the room needs to be ready
        int team1 = 0, team2 = 0;
        foreach (PlayerListing PL in playerListings)
        {
            if (PL.ReturnTeamInt == 1) team1++;
            else team2++;
            if (!PL.CheckIsReady)
            {
                SetServerMessage($"All players need to ready up before you can start a game!", localMessageColor);
                return;
            }
        }
        // The start condition must be met
        if (!StartConditionMet(PhotonGames.master.ReturnGameByIndex(gameType.value), team1, team2)) return;


        GameType gameTypeToLoad = PhotonGames.master.ReturnGameByIndex(gameType.value);
        PhotonNetwork.LoadLevel(gameTypeToLoad.sceneIndex);
    }
    
    public void ChangeReadyStatus(bool ready)
    {
        ExitGames.Client.Photon.Hashtable newHash = new ExitGames.Client.Photon.Hashtable();
        // Set r (ready) to match the Toggle value
        newHash.Add("r", ready);
        PhotonNetwork.LocalPlayer.SetCustomProperties(newHash);
    }
    public void SetTeam()
    {
        int onTeam = team.value + 1; 
        ExitGames.Client.Photon.Hashtable newHash = new ExitGames.Client.Photon.Hashtable();
        // Set t (team) to match the dropdown value
        newHash.Add("t", onTeam);
        PhotonNetwork.LocalPlayer.SetCustomProperties(newHash);
    }
    #endregion UI functions
    #region Other

    /// <summary>
    /// Sets the game type options for the dropdown menu
    /// </summary>
    void SetGameTypeOptions()
    {
        initGameTypeComplete = false;
        gameType.ClearOptions();
        if (PhotonGames.master == null)
        {
            Debug.LogError("No PhotonGames component was found!");
            gameType.enabled = false;
            maxPlayers.enabled = false;
            return;
        }
        GameType[] games = PhotonGames.master.ReturnGames;
        for (int i = 0; i < games.Length; i++)
        {
            TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
            newData.text = games[i].name;
            gameType.options.Add(newData);
        }
        gameType.RefreshShownValue();
        ChangedGameType();
        initGameTypeComplete = true;

        UpdateMaxPlayers();
    }
    void StringCheck(StringType type)
    {
        string stringToCheck = string.Empty;
        string typeOfField = string.Empty;
        if (type == StringType.UserName)
        {
            stringToCheck = userName.text;
            typeOfField = "Username";
        }
        else if (type == StringType.RoomName)
        {
            stringToCheck = createRoomInputField.text;
            typeOfField = "Room name";
        }
        if (stringToCheck == string.Empty || stringToCheck == " ")
        {
            SetUpInputFields(type);
            SetServerMessage($"{typeOfField} field is empty. A {typeOfField.ToLower()} needs to be at least 3 characters long.", localMessageColor);
            return;
        }
        if (stringToCheck.Length < 3)
        {
            SetUpInputFields(type);
            SetServerMessage($"The {typeOfField} is to chort. A {typeOfField.ToLower()} needs to be at least 3 characters long.", localMessageColor);
            return;
        }
        Regex allowedChars = new Regex("[^a-zA-Z0-9 ]");
        if (allowedChars.IsMatch(stringToCheck))
        {
            SetUpInputFields(type);
            SetServerMessage($"The {typeOfField} contains forbidden characters. Only letters and numbers are allowed.", localMessageColor);
            return;
        }
        if (type == StringType.UserName)
        {
            if (PlayerPrefs.HasKey("UserName") && stringToCheck == PlayerPrefs.GetString("UserName")) return;
            SetServerMessage($"New username set. GLHF {stringToCheck}!", localMessageColor);
            PlayerPrefs.SetString("UserName", stringToCheck);
        }
        else if (type == StringType.RoomName)
        {
            if (PlayerPrefs.HasKey("RoomName") && stringToCheck == PlayerPrefs.GetString("RoomName")) return;
            SetServerMessage($"New room name set!", localMessageColor);
            PlayerPrefs.SetString("RoomName", stringToCheck);
        }

    }
    bool StartConditionMet(GameType gameType, int team1, int team2)
    {
        bool returnValue = false;
        switch (gameType.startCondition)
        {
            case StartCondition.Any:
                returnValue = true;
                break;
            case StartCondition.EvenTeams:
                if (team1 == team2) returnValue = true;
                else
                {
                    SetServerMessage("Teams need to be even to start the game.", localMessageColor);
                    returnValue = false;
                }
                break;
            case StartCondition.EvenTeamsFull:
                if (team1 == team2 && team1 + team2 == gameType.maxPlayers) returnValue = true;
                else
                {
                    if (team1 != team2) SetServerMessage("Teams need to be even to start the game.", localMessageColor);
                    if (team1 + team2 != gameType.maxPlayers) SetServerMessage("Both teams need to be full to start the game.", localMessageColor);
                    returnValue = false;
                }
                break;
            case StartCondition.MaxOneDiff:
                if (Mathf.Abs(team1 - team2) <= 1) returnValue = true;
                else
                {
                    SetServerMessage("Max one player difference between the teams is allowed to start the game.", localMessageColor);
                    returnValue = false;
                }
                break;
            case StartCondition.MinOnePlayerPerTeam:
                if (team1 > 0 && team2 > 0) returnValue = true;
                else
                {
                    SetServerMessage("Min one player per team is needed to start the game.", localMessageColor);
                    returnValue = false;
                }
                break;
        }
        return returnValue;
    }
    public void SetServerMessage(string message, Color color)
    {
        GameObject newMessage = Instantiate(serverMessagePrefab, serverMessageContent);
        newMessage.GetComponent<ServerMessage>().SetText(message, color);
    }

    #endregion Other
    #region Called from ConnectPhoton

    public void PlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        foreach (PlayerListing PL in playerListings)
        {
            if (PL.compareUserID(targetPlayer.UserId))
            {
                if (changedProps.ContainsKey("r"))
                {
                    bool isReady = (bool)changedProps["r"];
                    // If this is the the localplayer
                    if (targetPlayer == PhotonNetwork.LocalPlayer)
                    {
                        // If player is ready, disable the team swap dropdown. If player isn't ready, enable the team swap dropdown
                        team.enabled = !isReady;
                    }
                    PL.UpdateReady(isReady);
                }
                if (changedProps.ContainsKey("t")) PL.UpdateTeam((int)changedProps["t"]);
            }
        }
    }
    public void LeftRoom()
    { 
        SetServerMessage("You left the room!", serverSideMessageColor);
        inRoomPanel.gameObject.SetActive(false);
        notInRoomPanel.gameObject.SetActive(true);
        userName.enabled = true;
        initMaxPlayersComplete = false;
    }
    public void CreateRoomFailed(int errorCode, string errorMessage)
    {
        createRoomButton.interactable = true;
        if(errorCode == 32766) SetServerMessage($"Failed to create a room : A room with the same name already exists. Choose another room name.", serverSideMessageColor);
        else SetServerMessage($"Failed to create a room : {errorMessage}", serverSideMessageColor);
    }
    public void CreateRoom()
    {
        createRoomButton.interactable = false;
        string roomName = "New Room";
        if (PlayerPrefs.HasKey("RoomName")) roomName = PlayerPrefs.GetString("RoomName");
        string ownerName = "Unknown User";
        if (PlayerPrefs.HasKey("UserName")) ownerName = PlayerPrefs.GetString("UserName");

        GameType[] games = PhotonGames.master.ReturnGames;
        int maxPlayersInRoom = games[gameType.value].minPlayers + (maxPlayers.value * 2);
        ConnectPhoton.master.CreateRoom(roomName, maxPlayersInRoom);
    }
    internal void Disconnected(DisconnectCause cause)
    {
        SetServerMessage($"Disconnected due to : {cause.ToString()}.", serverSideMessageColor);
        connectingText.gameObject.SetActive(true);
        inRoomPanel.gameObject.SetActive(true);
        notInRoomPanel.gameObject.SetActive(false);
    }
    public void ConnectedToMaster()
    {
        createRoomButton.interactable = true;
        connectingText.gameObject.SetActive(false);
        SetServerMessage("Connected to the server!", serverSideMessageColor);
    }
    /// <summary>
    /// This is called from OnCreatedRoom which is unreliable. Run in JoinedRoom if possible.
    /// </summary>
    public void CreatedRoom()
    {
        
    }

    public void PlayerEnteredRoom(Player newPlayer)
    {
        GameObject listing = Instantiate(playerListingPrefab, playerListingContent);
        PlayerListing thisListing = listing.GetComponent<PlayerListing>();

        ExitGames.Client.Photon.Hashtable thisPlayerHash = newPlayer.CustomProperties;
        thisListing.SetValues(newPlayer.NickName, thisPlayerHash.ContainsKey("r") ? (bool)thisPlayerHash["r"] : false, 1, newPlayer.IsMasterClient, newPlayer.IsLocal, newPlayer.UserId);
        playerListings.Add(thisListing);
        SetServerMessage($"{newPlayer.NickName} entered the room.", serverSideMessageColor);
        this.playersInRoom.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
    }
    public void PlayerLeftRoom(Player otherPlayer)
    {
        for(int i = 0; i < playerListings.Count; ++i)
        {
            // If this is the playerlisting of the user that left
            if(playerListings[i].compareUserID(otherPlayer.UserId))
            {
                Destroy(playerListings[i].gameObject);
                playerListings.RemoveAt(i);
                SetServerMessage($"{otherPlayer.NickName} left the room.", serverSideMessageColor);
                break;
            }
        }
        this.playersInRoom.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
        
    }
    public void JoinRoomFailed(short returnCode, string message)
    {
        SetServerMessage($"Joining room failed: {message}.", serverSideMessageColor);
    }
    public void MasterClientSwitched(Player newMasterClient)
    {
        // Loop through all the playerListings and set the master/ownder bool accordingly
        foreach(PlayerListing p in playerListings)
        {
            p.SetMaster(p.compareUserID(newMasterClient.UserId));
        }
        string newServerMessage = PhotonNetwork.IsMasterClient ? "You are the new room-owner!" : $"{newMasterClient.NickName} is the new room-owner!";
        SetServerMessage(newServerMessage, serverSideMessageColor);
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.enabled = true;
            startButton.interactable = true;
        }
    }
    public void JoinedRoom(string roomName)
    {
        playerListings.Clear();
        foreach(Transform t in playerListingContent)
        {
            Destroy(t.gameObject);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.enabled = false;
            startButton.interactable = false;
            SetServerMessage($"You joined the room '{roomName}'!", serverSideMessageColor);
        }
        else
        {
            startButton.enabled = true;
            startButton.interactable = true;
            SetServerMessage("You created a room!", serverSideMessageColor);
        }
        this.roomName.text = roomName;
        this.playersInRoom.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        inRoomPanel.gameObject.SetActive(true);
        notInRoomPanel.gameObject.SetActive(false);
        userName.enabled = false;
        readyToggle.isOn = false;
        team.value = 0;
        team.enabled = true;

        ExitGames.Client.Photon.Hashtable newHash = new ExitGames.Client.Photon.Hashtable();
        // Set r (ready) to false
        newHash.Add("r", false);
        // Set t (team) to 1
        newHash.Add("t", 1);
        PhotonNetwork.LocalPlayer.CustomProperties = newHash;

        // instantiate all 
        Dictionary<int, Player> playersInRoom = PhotonNetwork.CurrentRoom.Players;
        foreach (KeyValuePair<int, Player> KVP in playersInRoom)
        {
            GameObject listing = Instantiate(playerListingPrefab, playerListingContent);
            PlayerListing thisListing = listing.GetComponent<PlayerListing>();
            ExitGames.Client.Photon.Hashtable thisPlayerHash = KVP.Value.CustomProperties;
            if (PhotonNetwork.IsMasterClient) thisListing.SetValues(KVP.Value.NickName, thisPlayerHash.ContainsKey("r") ? (bool)thisPlayerHash["r"] : false, 1, KVP.Value.IsMasterClient, KVP.Value.IsLocal, KVP.Value.UserId);
            else
            {
                thisListing.SetValues(KVP.Value.NickName, thisPlayerHash.ContainsKey("r") ? (bool)thisPlayerHash["r"] : false, thisPlayerHash.ContainsKey("t") ? (int)thisPlayerHash["t"] : 1, KVP.Value.IsMasterClient, KVP.Value.IsLocal, KVP.Value.UserId);
            }
            playerListings.Add(thisListing);
        }
    }
    public void RoomListUpdated(List<RoomInfo> roomList)
    {
        if (roomListUpdateCooldownTimer <= 0)
        {
            roomListUpdateCooldownTimer = 2f;
            SetServerMessage("Roomslist updated!", serverSideMessageColor);
        }
        foreach (RoomInfo RI in roomList)
        {
            if (RI.RemovedFromList)
            {
                foreach (RoomListing RL in roomListings)
                {
                    if (RL.ReturnRoomName == RI.Name)
                    {
                        roomListings.Remove(RL);
                        Destroy(RL.gameObject);
                    }
                }
            }
            else
            {
                bool matchFound = false;
                foreach (RoomListing RL in roomListings)
                {
                    if (RL.ReturnRoomName == RI.Name)
                    {
                        matchFound = true;
                        RL.SetValues(RI.Name, RI.PlayerCount + "/" + RI.MaxPlayers);
                        break;
                    }
                }
                if (!matchFound)
                {
                    GameObject listing = Instantiate(roomListingPrefab, roomsListingContent);
                    RoomListing thisListing = listing.GetComponent<RoomListing>();

                    thisListing.SetValues(RI.Name, RI.PlayerCount + "/" + RI.MaxPlayers);
                    roomListings.Add(thisListing);
                }

            }
        }
    }
    #endregion Called from ConnectPhoton
}
