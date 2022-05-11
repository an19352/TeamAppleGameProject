using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFractured : MonoBehaviour
{
    // This script breaks the breakable platforms
    public float respawnTimer;
    public GameObject brokenVersion;
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Movement>().FractureBoard(transform.position); // Tell the player to initiate the destruction protocol for this platfom over the metwork
        }                                                                                // This way, this platform can be spawned in without any sync
    }

    // Explode the board and start the coroutines that spawn an identical one in 5 seconds
    public void Fracture()
    {
        GameObject bv = Instantiate(brokenVersion, transform.position, transform.rotation);
        transform.Find("BoardPieces").gameObject.SetActive(false);  
        StartCoroutine("RespawnFlag");
        StartCoroutine(DestroyDebris(bv));
    }

    IEnumerator RespawnFlag()
    {
        yield return new WaitForSeconds(respawnTimer);
        transform.GetChild(0).gameObject.SetActive(true);
    }
    IEnumerator DestroyDebris(GameObject bv)
    {
        yield return new WaitForSeconds(5);
        Destroy(bv);
    }
}
