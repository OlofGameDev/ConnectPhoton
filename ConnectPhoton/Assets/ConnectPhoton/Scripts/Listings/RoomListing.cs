using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomName, roomOwner, players;
    
    public void SetValues(string roomName, string roomOwner, string players)
    {
        this.roomName.text = roomName;
        this.roomOwner.text = roomOwner;
        this.players.text = players;
    }
    public string ReturnRoomName
    {
        get { return roomName.text; }
    }
    public void JoinRoom()
    {
        Debug.Log("Trying to join room");
        ConnectPhoton.master.JoinRoom(roomName.text);
    }
}
