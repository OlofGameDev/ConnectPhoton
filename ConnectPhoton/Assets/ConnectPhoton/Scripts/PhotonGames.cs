using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

// Conditions to allow the room owner to start a game
public enum StartCondition
{
    // No conditions to start a game
    Any,
    // At least one player per team
    MinOnePlayerPerTeam,
    // Max difference is one more player on either team
    MaxOneDiff,
    // Teams have to be even
    EvenTeams,
    // Both teams must be full and teams must be even
    EvenTeamsFull,
}

[System.Serializable]
public class GameType
{
    public string name;
    [TextArea(2,8)]
    public string description;
    public int minPlayers = 2, maxPlayers = 2;
    public int sceneIndex;
    public StartCondition startCondition;
}
public class PhotonGames : MonoBehaviour
{
    [SerializeField] GameType[] games;
    public static PhotonGames master;
    
    public GameType[] ReturnGames { get { return games; } }

    public GameType ReturnGameByIndex(int index) 
    { 
        return games[index];
    }
    private void Awake()
    {
        if (master != null) Destroy(this);
        master = this;


    }
}
