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
        mesh.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // If it got triggered by an object with a Movement file on it, 
        // checks for the player ID to score a point for the enemy team
        // and respawn the player
        if (other.CompareTag("Player"))
        {
            Movement mov = other.GetComponent<Movement>();
            int team = (1 + gameMechanics.checkTeam(mov.GetId())) % 2;    // Only works for 2 teams
            gameMechanics.RPC_Score(team);
            mov.Spawn();
        }
    }
}
