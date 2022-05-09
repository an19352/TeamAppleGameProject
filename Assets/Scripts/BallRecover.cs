using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallRecover : MonoBehaviour
{
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
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            if (!player.GetComponent<FlagHolder>().enabled)
            {
                gameMechanics.RPC_EnableFlagHolder(player.GetComponent<Movement>().GetId(), teamID);
                PV.RPC("DisableDroppedFlag", RpcTarget.All);
                gameMechanics.drop = new Vector3(0,0,0);
            }

        }
    }
    [PunRPC]
    void DisableDroppedFlag()
    {
        this.gameObject.SetActive(false);
    }

}
