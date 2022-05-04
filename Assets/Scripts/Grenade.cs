using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    public float delay;
    public float radius;
    public float force;
    public GameObject explosionEffect;
    float countdown;
    bool hasExploded = false;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }
    void Explode()
    {
        // show effect
        GameObject explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, transform.rotation);
        Debug.Log("explosion instantiated");
        //get nearbyb objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        
        // Add force
        foreach (Collider nearbyObject in colliders)
        {
            //Debug.Log(nearbyObject.tag);
            rb = nearbyObject.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                if (nearbyObject.tag == "Generator") nearbyObject.GetComponent<EnergyGenerator>().applyForce(force);
                if (nearbyObject.tag == "Turret") nearbyObject.GetComponent<Turret>().applyForce(force);
                rb.AddExplosionForce(force, transform.position, radius);
            }
        }
        // remove grenade
        Destroy(gameObject);
        Destroy(explosion);
    }
}
