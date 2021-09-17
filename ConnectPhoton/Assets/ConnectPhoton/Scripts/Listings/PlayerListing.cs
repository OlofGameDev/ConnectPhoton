using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName, ready, team, owner;
    [SerializeField] Color readyColor, notReadyColor, isLocalColor;
    string UserID;
    bool readyBool = false;
    int teamInt = 1;

    public int ReturnTeamInt {  get { return teamInt; } }
    public string ReturnPlayerName { get { return playerName.text; } }
    public void SetValues(string playerName, bool ready, int team, bool owner, bool isLocal, string userID)
    {
        this.playerName.text = playerName;
        this.ready.text = ready ? "Ready" : "Not Ready";
        readyBool = ready;
        this.team.text = team.ToString();
        teamInt = team;
        this.ready.color = ready ? readyColor : notReadyColor;
        this.owner.text = owner ? "Owner" : " ";
        // If this is the local/player's listing, change the color to make it stick out
        if (isLocal) transform.GetComponent<Image>().color = isLocalColor;

        this.UserID = userID;
    }
    public bool CheckIsReady { get { return readyBool; } }
    public void UpdateReady(bool ready)
    {
        readyBool = ready;
        this.ready.text = ready ? "Ready" : "Not Ready";
        this.ready.color = ready ? readyColor : notReadyColor;
    }
    public void UpdateTeam(int team)
    {
        this.team.text = team.ToString();
        teamInt = team;
    }
    public bool compareUserID(string otherUser)
    {
        if (otherUser == UserID) return true;
        else return false;
    }
    public void SetMaster(bool isMaster)
    {
        owner.text = isMaster ? "Owner" : " ";
    }
}
