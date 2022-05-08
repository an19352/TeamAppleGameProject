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
            other.gameObject.GetComponent<Movement>().FractureBoard(transform.position);
        }
    }

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
