using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    void Start()
    {
        GameMechanics.gameMechanics.spawnPpoints.Add(transform);
    }
}
