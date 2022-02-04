using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPreFab;
    public GameMechannicsPhoton gameMechannics;

    public float minX, maxX, minZ, maxZ;



    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 6, Random.Range(minZ, maxZ));
        GameObject player = PhotonNetwork.Instantiate(playerPreFab.name, randomPosition, Quaternion.identity);
        gameMechannics.addPlayer(player, -1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
