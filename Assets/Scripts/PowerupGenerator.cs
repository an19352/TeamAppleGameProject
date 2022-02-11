using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupGenerator : MonoBehaviour
{
    ObjectPooler poolOfObject;
    
    private GameObject newCube;
    private List<string> powerupTags;

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
    void GenerateRandomPowerup()
    {
        int randPow = Random.Range(0, powerupTags.Count);
        
        newCube = poolOfObject.SpawnFromPool(powerupTags[randPow], RandomPosition(), Quaternion.identity);
        newCube.SetActive(true);
    }

    //Coroutine to start generating random powerups every 5-10 seconds
    IEnumerator StartGenerator()
    {
        while (true)
        {
            float time = Random.Range(5, 10);
            yield return new WaitForSeconds(time);
            GenerateRandomPowerup();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        poolOfObject = ObjectPooler.OP;

        powerupTags = new List<string>();
        foreach (ObjectPooler.Pool pool in poolOfObject.pools)
            if (pool.prefab.CompareTag(powerupTag))
                powerupTags.Add(pool.tag);

        ParentPowerups();
        StartCoroutine(StartGenerator());
    }

    void ParentPowerups()
    {
        foreach (KeyValuePair<string, Queue<GameObject>> kvp in poolOfObject.poolDictionary)
            if (kvp.Value.Peek().CompareTag(powerupTag))
                foreach (GameObject powerup in kvp.Value)
                    powerup.transform.SetParent(transform);
    }
}
