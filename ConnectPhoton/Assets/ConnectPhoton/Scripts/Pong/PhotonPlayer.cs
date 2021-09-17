using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PhotonPlayer : MonoBehaviour
{
    PhotonView photonView;
    [SerializeField] int team, teamIndex;
    [SerializeField] Transform spawnPoint;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    public void SetValues(int team, int teamIndex, Transform spawnPoint)
    {
        this.team = team;
        this.teamIndex = teamIndex;
        this.spawnPoint = spawnPoint;
    }
}
