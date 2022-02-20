using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerupGenerator : MonoBehaviour
{
    ObjectPooler poolOfObject;
    
    private GameObject newCube;
    private List<string> powerupTags;
    private PhotonView PV;

    public string powerupTag = "Powerup";
    public List<Transform> spawiningPoints;

    //Generates random position in the space of Vector3
    private Vector3 RandomPosition()
    {
        int index = Random.Range(0, spawiningPoints.Count);
        return spawiningPoints[index].position;
    }

    //Places a random powerup in a random position
    [PunRPC]
    void GenerateRandomPowerup(int randPow, Vector3 randPosition)
    {
        newCube = poolOfObject.SpawnFromPool(powerupTags[randPow], randPosition, Quaternion.identity);
        newCube.SetActive(true);
    }

    //Coroutine to start generating random powerups every 5-10 seconds
    IEnumerator StartGenerator()
    {
        while (true)
        {
            float time = Random.Range(5, 10);
            yield return new WaitForSeconds(time);

            int randPow = Random.Range(0, powerupTags.Count);
            Vector3 randPosition = RandomPosition();
            if (PV.IsOwnerActive)
                PV.RPC("GenerateRandomPowerup", RpcTarget.All, randPow, randPosition);
            else
                GenerateRandomPowerup(randPow, randPosition);
            
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();

        if (!PhotonNetwork.IsMasterClient) return;

        poolOfObject = ObjectPooler.OP;

        powerupTags = new List<string>();
        foreach (ObjectPooler.Pool pool in poolOfObject.pools)
            if (pool.prefab.CompareTag(powerupTag))
                powerupTags.Add(pool.tag);

        PV.RPC("ParentPowerups", RpcTarget.AllBuffered);
        StartCoroutine(StartGenerator());
    }

    [PunRPC]
    void ParentPowerups()
    {
        foreach (KeyValuePair<string, Queue<GameObject>> kvp in poolOfObject.poolDictionary)
            if (kvp.Value.Peek().CompareTag(powerupTag))
                foreach (GameObject powerup in kvp.Value)
                    powerup.transform.SetParent(transform);
    }
}
