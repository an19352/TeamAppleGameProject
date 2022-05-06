using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallRecover : MonoBehaviour
{
    PhotonView PV;
    public int teamID;
    public static GameMechanics gameMechanics;


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
            // if (!player.GetComponent<FlagHolder>().enabled)
            {
                GameMechanics.gameMechanics.RPC_EnableFlagHolder(player.GetComponent<Movement>().GetId());
                PV.RPC("DisableDroppedFlag", RpcTarget.All);
            }

        }
    }
    [PunRPC]
    void DisableDroppedFlag()
    {
        this.gameObject.SetActive(false);
    }

}
