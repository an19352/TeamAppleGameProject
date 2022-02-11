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

    //Generates random position in the space of Vector3
    private Vector3 RandomPosition()
    {
        float x = Random.Range(-10, 10);
        float z = Random.Range(-10, 10);
        Vector3 pos = new Vector3(x, 1f, z);
        return pos;
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
            int numberOfPlayers = PhotonNetwork.CountOfPlayers;
            if (numberOfPlayers == 0) numberOfPlayers++;
            float time = Random.Range(5 * numberOfPlayers, 10 * numberOfPlayers);
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

        if (PV.Owner.IsMasterClient)
        {
            poolOfObject = ObjectPooler.OP;

            powerupTags = new List<string>();
            foreach (ObjectPooler.Pool pool in poolOfObject.pools)
                if (pool.prefab.CompareTag(powerupTag))
                    powerupTags.Add(pool.tag);

            ParentPowerups();
            StartCoroutine(StartGenerator());
        }
    }

    void ParentPowerups()
    {
        foreach (KeyValuePair<string, Queue<GameObject>> kvp in poolOfObject.poolDictionary)
            if (kvp.Value.Peek().CompareTag(powerupTag))
                foreach (GameObject powerup in kvp.Value)
                    powerup.transform.SetParent(transform);
    }
}
