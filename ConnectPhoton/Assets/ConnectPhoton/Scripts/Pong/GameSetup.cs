using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameSetup : MonoBehaviourPun
{
    public static GameSetup master;

    public Transform[] spawnPointsTeam1;
    public Transform[] spawnPointsTeam2;
    [SerializeField] List<PhotonPlayer> players = new List<PhotonPlayer>();
    int team1Index, team2Index;
    [SerializeField] string path = "Pong";
    [SerializeField] string avatarName = "PlayerAvatar";
    [SerializeField] int sendRate = 30, serializationRate = 10;

    private void Awake()
    {
        if (master != null) Destroy(this);
        master = this;
        PhotonNetwork.SendRate = sendRate;
        PhotonNetwork.SerializationRate = serializationRate;
    }

    [PunRPC]
    void SpawnPlayer(Photon.Realtime.Player player, int team, int teamIndex)
    {
        // The RPC call will be sent out to all clients. Make sure this is the right one
        if (player != PhotonNetwork.LocalPlayer) return;

        Transform spawnPoint;
        spawnPoint = team == 1 ? spawnPointsTeam1[teamIndex] : spawnPointsTeam2[teamIndex];

        GameObject newPlayer = PhotonNetwork.Instantiate(Path.Combine(path, avatarName), spawnPoint.position, spawnPoint.rotation, 0);
        PhotonPlayer PP = newPlayer.GetComponent<PhotonPlayer>();
        PP.SetValues(team, teamIndex, spawnPoint);
        players.Add(PP);
    }

    [PunRPC] 
    void IncrementTeamIndex(Photon.Realtime.Player player, int team)
    {
        int teamIndex;
        if (team == 1)
        {
            teamIndex = team1Index;
            ++team1Index;
        }
        else
        {
            teamIndex = team2Index;
            ++team2Index;
        }
        // Send a RPC call to all clients
        photonView.RPC("SpawnPlayer", RpcTarget.All, player, team, teamIndex);
    }
    public void AddPlayer(Photon.Realtime.Player player)
    {
        int team;
        ExitGames.Client.Photon.Hashtable thisPlayerHash = player.CustomProperties;
        if (thisPlayerHash.ContainsKey("t"))
        {
            team = (int)thisPlayerHash["t"];
        }
        else
        {
            Debug.LogError("No team found in player custom properties.");
            return;
        }
        // Send a RPC call to the master client to get a unique spawn point
        photonView.RPC("IncrementTeamIndex", RpcTarget.MasterClient, player, team);
    }
}
