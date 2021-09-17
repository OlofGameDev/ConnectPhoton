using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(GameSetup))]
[RequireComponent(typeof(PhotonView))]
public class SpawnPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameSetup.master.AddPlayer(PhotonNetwork.LocalPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
