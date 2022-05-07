using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFractured : MonoBehaviour
{
    public float respawnTimer;
    public GameObject brokenVersion;
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject bv = Instantiate(brokenVersion, transform.position, transform.rotation);
            transform.Find("BoardPieces").gameObject.SetActive(false);
            StartCoroutine("RespawnFlag");
            StartCoroutine(DestroyDebris(bv));
        };
    }
    IEnumerator RespawnFlag()
    {
        yield return new WaitForSeconds(respawnTimer);
        transform.Find("BoardPieces").gameObject.SetActive(true);
    }
    IEnumerator DestroyDebris(GameObject bv)
    {
        yield return new WaitForSeconds(5);
        Destroy(bv);
    }
}
