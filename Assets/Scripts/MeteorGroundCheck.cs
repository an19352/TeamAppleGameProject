using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MeteorGroundCheck : MonoBehaviour
{
    // Stops meteor from falling when they hit ground and stuns nearby players
    public float meteorForce;
    public float meteorRadius;
    public float hitRadius;
    private PhotonView PV;
    public float stunTime;
    public GameObject explosion;
    public GameObject stunEffect;


    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    void Update()
    {

    }

    private void OnCollisionEnter(Collision col)
    {
        PhotonNetwork.Instantiate(explosion.name, transform.position, transform.rotation);
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, hitRadius);
        //Debug.Log("drewsphere");
        foreach (Collider nearby in collidersInRadius)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (nearby.CompareTag("Generator"))
                {
                    nearby.GetComponent<EnergyGenerator>().applyForce(meteorForce);
                }

                if (nearby.CompareTag("Turret"))
                {
                    nearby.GetComponent<Turret>().applyForce(meteorForce);
                }
            }
        }

        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, meteorRadius);
        foreach (Collider player in playersInRadius)
        {
            if (player.CompareTag("Player"))
            {
                StunPlayer(player);
            }
        }
        gameObject.SetActive(false);
        //StartCoroutine("DespawnMeteor");
    }

    void StunPlayer(Collider player)
    {
        player.gameObject.GetComponent<Movement>().DisableMoveSec(stunTime);
        PhotonNetwork.Instantiate(stunEffect.name, player.transform.position, player.transform.rotation);
    }

    /*IEnumerator CoStunPlayer(Collider player)
    {
        player.GetComponent<Movement>().isNPC = true;
        Debug.Log("disabled");
        Debug.Log("waiting");
        yield return new WaitForSeconds(3);
        Debug.Log("waited");
        player.GetComponent<Movement>().isNPC = false;
        Debug.Log("enabled");
        transform.gameObject.SetActive(false);
        yield return null;
    }

    void RPC_StunPlayer(Collider player)
    {
        StartCoroutine(CoStunPlayer(player));
        //PV.RPC("StunPlayer", RpcTarget.All, player);
    }

    IEnumerator DespawnMeteor()
    {
        yield return new WaitForSeconds(3);
        gameObject.SetActive(false);
    }*/
}
