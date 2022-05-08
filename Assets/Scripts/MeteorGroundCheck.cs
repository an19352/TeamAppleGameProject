using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MeteorGroundCheck : MonoBehaviour
{

    public GameObject explosionEffect;
    public float meteorForce;
    public float meteorRadius;
    public float meteorPushForce;
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
        Rigidbody rb = col.GetContact(0).otherCollider.GetComponent<Rigidbody>();
        Collider other = col.GetContact(0).otherCollider;
        Debug.Log(rb);
        if (rb != null)
        {
            if (other.CompareTag("Generator"))
            {
                other.GetComponent<EnergyGenerator>().applyForce(meteorForce);
            }
    
            if (other.CompareTag("Turret"))
            {
                other.GetComponent<Turret>().applyForce(meteorForce);
            }
        }
            
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, meteorRadius, ~0, 0);
        foreach (Collider player in playersInRadius)
        {
            if (player.CompareTag("Player"))
            {
                RPC_StunPlayer(player);
            }
        }

        StartCoroutine("DespawnMeteor");
    }
    
    [PunRPC]
    void StunPlayer(Collider player)
    {
        player.GetComponent<Movement>().isNPC = true;
        StartCoroutine(CoStunPlayer(player));
    }

    IEnumerator CoStunPlayer(Collider player)
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
    }
}
