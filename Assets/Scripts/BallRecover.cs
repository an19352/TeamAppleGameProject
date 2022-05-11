using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallRecover : MonoBehaviour
{
    // This script helps the dropped flag identify it has been recovered
    PhotonView PV;
    public int teamID;
    GameMechanics gameMechanics;


    void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;
    }

    void OnTriggerEnter(Collider other)
    {
        // teamID represents the team this flag was taken from and it's assigned to it by the player that droped the ball
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            if (!player.GetComponent<FlagHolder>().enabled)
            {
                gameMechanics.RPC_EnableFlagHolder(player.GetComponent<Movement>().GetId(), teamID); // Game Mechanics FTW
                PV.RPC("DisableDroppedFlag", RpcTarget.All);
                gameMechanics.drop = new Vector3(0,0,0);
            }
        }
    }

    [PunRPC] //Tells every client to disable this gameObject
    void DisableDroppedFlag()
    {
        this.gameObject.SetActive(false);
    }

}
