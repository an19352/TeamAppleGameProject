using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRecover : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            if (!player.GetComponent<FlagHolder>().enabled)
            {
                GameMechanics.gameMechanics.RPC_EnableFlagHolder(player.GetComponent<Movement>().GetId());
                Destroy(this.gameObject);
            }
        }
    }
}
