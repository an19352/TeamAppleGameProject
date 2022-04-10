using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallRecover : MonoBehaviour
{
    PhotonView PV;
    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            if (!player.GetComponent<FlagHolder>().enabled)
            {
                GameMechanics.gameMechanics.RPC_EnableFlagHolder(player.GetComponent<Movement>().GetId());
                GameMechanics.gameMechanics.RPC_Destroy(gameObject);
            }
        }
    }
}
