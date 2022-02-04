using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupGenerator : MonoBehaviour
{
    
    private GameObject newCube;
    private GameObject[] powerups;
    
    
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
        int randPow = Random.Range(0, powerups.Length);
        newCube = Object.Instantiate(
            powerups[randPow],
            RandomPosition(),
            new Quaternion(0f, 0f, 0f, 0f),
            transform
        );
        newCube.SetActive(true);
    }
    
    //Deactivates all powerup gameobjects so the originals aren't visible
    void DeactivatePowerups()
    {
        for (int i = 0; i < powerups.Length; i++)
        {
            powerups[i].SetActive(false);
        }
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
        powerups = GameObject.FindGameObjectsWithTag("Powerup");
        DeactivatePowerups();
        StartCoroutine(StartGenerator());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
