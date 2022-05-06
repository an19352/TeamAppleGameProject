using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameMechanics.gameMechanics.spawnPpoints.Add(transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
