using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MeteorGroundCheck : MonoBehaviour
{
    
    public float meteorForce;
    public float meteorRadius;
    public float hitRadius;
    private PhotonView PV;
    public float stunTime;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision col)
    {
        //GameObject explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, transform.rotation);
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, hitRadius);
        Debug.Log("drewsphere");
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
