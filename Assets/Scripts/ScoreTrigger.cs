using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    public static GameMechanics gameMechanics;

    // Hides the mesh of our Trigger
    void Start()
    {
        gameMechanics = GameMechanics.gameMechanics;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        // mesh.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // If it got triggered by an object with a Movement file on it, 
        // checks for the player ID to score a point for the enemy team
        // and respawn the player
        bool offline = gameMechanics == null;
        if (other.CompareTag("Player"))
        {
            Movement mov = other.GetComponent<Movement>();

            // if the player also holds the flag, respawn the flag at the nearest platform
            FlagHolder fh = other.GetComponent<FlagHolder>();
            if (!offline)
            {
                if (fh.enabled == true)
                {
                    fh.RespawnFlag(other.transform.position, other.transform.rotation);
                    fh.enabled = false;
                }
            }

            // ! always do the flag respawn before player respawn, to capture the drop coordinate
            mov.Spawn();

            if (offline) return;

            gameMechanics.RPC_Score((gameMechanics.checkTeam(mov.GetId()) + 1)%2);
        }
    }
}
