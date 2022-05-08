using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MeteorGroundCheck : MonoBehaviour
{

    public GameObject explosionEffect;
    public float meteorForce;
    public float meteorRadius;
    public float meteorPushForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision col)
    {
        //GameObject explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, transform.rotation);
        transform.gameObject.SetActive(false);
        if (col.GetContact(0).otherCollider.transform.parent.gameObject != null)
        {
            GameObject other = col.GetContact(0).otherCollider.transform.parent.gameObject;
            Debug.Log(other);
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
                Vector3 pushFactor = (player.transform.position - transform.position).normalized * meteorPushForce;
                pushFactor.y = 0;
                player.GetComponent<Movement>().PushMe(pushFactor, ForceMode.Impulse, false);   
            }
        }
    }
}
